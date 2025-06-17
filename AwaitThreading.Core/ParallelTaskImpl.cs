// MIT License
// Copyright (c) 2023 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Runtime.CompilerServices;
using AwaitThreading.Core.Context;

namespace AwaitThreading.Core;

internal sealed class ParallelTaskImpl<T>
{
    [ThreadStatic] // TODO: clear?
    private static ParallelTaskResult<T>? _parallelResult;

    // note: volatile is not required
    private IContinuationInvoker? _continuation;

    private Action? _onDemandStartAction;

    public void SetResult(ParallelTaskResult<T> result)
    {
        _parallelResult = result;
        if (_continuation is not { } continuation)
        {
            throw new InvalidOperationException("Continuation should be set before result in parallel behaviour");
        }

        continuation.Invoke();
    }

    internal ParallelTaskResult<T> GetResult()
    {
        if (_continuation is null || !_parallelResult.HasValue)
        {
            Assertion.ThrowInvalidDirectGetResultCall();
        }

        return _parallelResult.Value;
    }

    public void ParallelOnCompleted<TStateMachine>(TStateMachine stateMachine) 
        where TStateMachine : IAsyncStateMachine
    {
        var onDemandStartAction = Interlocked.Exchange(ref _onDemandStartAction, null);
        if (onDemandStartAction is null)
        {
            Assertion.ThrowInvalidSecondAwaitOfParallelTask();
        }

        // continuation should run with the same context as onDemandStartAction finishes with (it can contain fork or join)
        _continuation = new ParallelContinuationInvoker<TStateMachine>(stateMachine);

        // clear the parallel context here since the thread will go to the thread pool after this method 
        var parallelContext = ParallelContextStorage.CaptureAndClear();  

        // onDemandStartAction should have the same parallelContext as we have at the moment of awaiting,
        // and since we run it via Task.Run, we should pass the context  
        Task.Run(
            () =>
            {
                ParallelContextStorage.Restore(parallelContext);
                try
                {
                    onDemandStartAction.Invoke();
                }
                finally
                {
                    ParallelContextStorage.ClearButNotExpected();
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
                    ParallelContextStorage.ClearButNotExpected();
                }
            });
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
            // Note: in general, original context (at the moment we are awaiting the `Task` method) should be empty,
            // except when we are in the sync part of async Task method. But even if it's not, we are clearing it before
            // yielding to async continuation of regular Task, since we can't afford ParallelContext to be passed to the 
            // regular Task method and therefore allow `await ParallelTask (fork) -> await Task -> await ParallelTask (join)`.
            // Otherwise, after exiting the `ParallelTask (join)`, we'll pass the control flow to the standard Task
            // method builder, and it can schedule the continuation on other thread and return the thread to the Thread
            // pool with ParallelContext set.
            // So, we always guarantee that nested ParallelTask method is executed with empty ParallelContext
            // at the beginning. When the nested ParallelTask method nevertheless finishes with some ParallelContext
            // (e.g. it contains fork), context will be cleared and only one thread will proceed,
            // raising `BadAwaitExceptionDispatchInfo` to the awaiter.
            var parallelContext = ParallelContextStorage.CaptureAndClear();

            var action = Interlocked.Exchange(ref _action, null);
            if (action is null)
            {
                // Already invoked action once. Continuation in standard async Task method builder can't be 
                // called twice, so we need to just return (unless we want to break the thread pool thread with 
                // an exception). But it can only happen when we've got a ParallelContext and, therefore, already
                // notified the continuation with 'BadAwaitExceptionDispatchInfo' in other thread.
                Debug.Assert(!parallelContext.IsEmpty);
                return;
            }

            if (!parallelContext.IsEmpty)
            {
                // Note: it works, but we rely on the fact that the same thread will run the continuation.
                // It's required for the forking workload, but it can be changed in the future for cases
                // when normal task awaits ParallelTask, so the continuation can be re-scheduled.
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
        _onDemandStartAction = () => stateMachineLocal.MoveNext(); //TODO: optimize allocations?
    }
}