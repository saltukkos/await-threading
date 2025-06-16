// MIT License
// Copyright (c) 2023 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace AwaitThreading.Core;

internal sealed class ParallelTaskImpl<T>
{
    [ThreadStatic] // TODO: clear?
    private static ParallelTaskResult<T>? _parallelResult;

    //private ParallelTaskResult<T>? _syncResult;

    // note: volatile is not required
    private IContinuationInvoker? _continuation;
    //private bool _shouldSupportStandardBehaviour;

    private Action? _onDemandStartAction;

    public void SetResult(ParallelTaskResult<T> result)
    {
        // if (_shouldSupportStandardBehaviour)
        // {
        //     _syncResult = result;
        //
        //     var oldValue = Interlocked.CompareExchange(ref _continuation, TaskFinishedMarker.Instance, null);
        //     if (oldValue != null)
        //     {
        //         Debug.Assert(!ReferenceEquals(oldValue, TaskFinishedMarker.Instance));
        //         oldValue.Invoke();
        //     }
        // }

        _parallelResult = result;
        if (_continuation is null)
        {
            Assertion.Fail("Continuation should be set before result in parallel behaviour");
        }

        _continuation.Invoke();
    }

    internal ParallelTaskResult<T> GetResult()
    {
        // if (!_shouldSupportStandardBehaviour)
        {
            if (_continuation is null || !_parallelResult.HasValue)
            {
                Assertion.ThrowInvalidDirectGetResultCall();
            }

            return _parallelResult.Value;
        }

        // if (!_syncResult.HasValue)
        // {
        //     Assertion.ThrowInvalidDirectGetResultCall();
        // }
        //
        // return _syncResult.Value;
    }

    public void ParallelOnCompleted<TStateMachine>(TStateMachine stateMachine) 
        where TStateMachine : IAsyncStateMachine
    {
        var onDemandStartAction = Interlocked.Exchange(ref _onDemandStartAction, null);
        if (onDemandStartAction is null)
        {
            Assertion.ThrowInvalidSecondAwaitOfParallelTask();
        }

        // continuation: should run with the same context as onDemandStartAction finishes with (it can contain fork or join)
        _continuation = new ParallelContinuationInvoker<TStateMachine>(stateMachine);

        var parallelContext = ParallelContext.CaptureAndClear();  

        // onDemandStartAction should have the same parallelContext as we have at the moment of awaiting,
        // so if we run it via Task.run, we should pass the context  
        Task.Run(
            () =>
            {
                ParallelContext.Restore(parallelContext);
                try
                {
                    onDemandStartAction.Invoke();
                }
                finally
                {
                    ParallelContext.CaptureAndClear(); // note: just in case, probably can get rid of it and write an assertion
                }
            });
    }

    public void OnCompleted(Action continuation)
    {
        OnCompletedInternal(continuation);
    }

    public void UnsafeOnCompleted(Action continuation)
    {
        // TODO: we need a proper implementation: one restores the execution context and another one doesn't
        OnCompleted(continuation);
    }

    private void OnCompletedInternal(Action continuation)
    {
        var onDemandStartAction = Interlocked.Exchange(ref _onDemandStartAction, null);
        if (onDemandStartAction is null)
        {
            Assertion.ThrowInvalidSecondAwaitOfParallelTask();
        }
        
        // TODO: continuationInvoker should check that context is empty after onDemandStartAction finishes.
        //  onDemandStartAction should start running with empty context

        var continuationInvoker = new RegularContinuationInvokerWithFrameProtection(continuation);

        _continuation = continuationInvoker;
        Task.Run(
            () =>
            {
                try
                {
                    onDemandStartAction.Invoke();
                }
                finally
                {
                    ParallelContext.ClearButNotExpected();
                }
            });

        // _shouldSupportStandardBehaviour = true;
        //
        // var oldContinuation = Interlocked.CompareExchange(ref _continuation, continuationInvoker, null);
        // if (oldContinuation == null)
        // {
        //     return;
        // }
        //
        // Debug.Assert(ReferenceEquals(oldContinuation, TaskFinishedMarker.Instance));
        // continuationInvoker.Invoke();
    }

    private sealed class RegularContinuationInvokerWithFrameProtection : IContinuationInvoker
    {
        private Action? _action;
        public RegularContinuationInvokerWithFrameProtection(Action action)
        {
            _action = action;
        }

        public void Invoke()
        {
            // Note: in general, original context (at the moment we are awaiting the `Task` method should be empty,
            // except when we are in the sync part of async Task method.
            // But fo continuation, we are going to clear it anyway. This behaviour is shown and explained in
            // Await_ParallelTaskHasUnpairedJoin_InvalidOperationExceptionIsThrows
            var parallelContext = ParallelContext.CaptureAndClear();

            var action = Interlocked.Exchange(ref _action, null);
            if (action is null)
            {
                // Already invoked action once. Continuation in standard async Task method builder can't be 
                // called twice, so we need to just return (unless we want to break the thread pool thread with 
                // an exception). But it can only happen when we've got a ParallelContext and, therefore, already
                // notified the continuation with 'BadAwaitExceptionDispatchInfo'
                Debug.Assert(!parallelContext.IsEmpty);
                return;
            }

            if (!parallelContext.IsEmpty)
            // if (!ParallelContext.GetCurrentContext().Equals(_contextBeforeAwait))
            {
                // Note: it works, but we rely on the fact that the same thread will run the continuation.
                // It's required for the forking workload, but it can be changed for cases when normal task
                // awaits ParallelTask, so the continuation can be re-scheduled
                _parallelResult = new ParallelTaskResult<T>(Assertion.BadAwaitExceptionDispatchInfo);
            }

            action.Invoke();
        }
    }

    /// <summary>
    /// Instead of running the state machine right now, we are preserving it to run later on-demand.
    /// With this approach, we can control parallel context set\clear depending on the way our task is awaited
    /// (e.g. using regular UnsafeOnCompleted or ParallelOnCompleted) 
    /// </summary>
    public void SetStateMachine<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
    {
        var stateMachineLocal = stateMachine;
        _onDemandStartAction = () => stateMachineLocal.MoveNext();
    }
}