// MIT License
// Copyright (c) 2023 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

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

        _continuation = new ParallelContinuationInvoker<TStateMachine>(stateMachine);
        onDemandStartAction.Invoke();
    }

    public void OnCompleted(Action continuation)
    {
        OnCompletedInternal(continuation);
    }

    public void UnsafeOnCompleted(Action continuation)
    {
        // TODO: do we need a proper implementation?
        OnCompleted(continuation);
    }

    private void OnCompletedInternal(Action continuation)
    {
        var parallelContext = ParallelContext.GetCurrentContext();
        var continuationInvoker = new RegularContinuationInvokerWithFrameProtection(continuation, parallelContext);

        var onDemandStartAction = Interlocked.Exchange(ref _onDemandStartAction, null);
        if (onDemandStartAction is null)
        {
            ParallelContext.Restore(parallelContext);
            Assertion.ThrowInvalidSecondAwaitOfParallelTask();
        }

        ParallelContext.CaptureAndClear();
        _continuation = continuationInvoker;
        onDemandStartAction.Invoke();

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
        private readonly Action _action;
        private readonly ParallelContext _contextBeforeAwait;
        public RegularContinuationInvokerWithFrameProtection(Action action, ParallelContext contextBeforeAwait)
        {
            _contextBeforeAwait = contextBeforeAwait;
            _action = action;
        }

        public void Invoke()
        {
            if (ParallelContext.GetCurrentFrameSafe() != null)
            // if (!ParallelContext.GetCurrentContext().Equals(_contextBeforeAwait))
            {
                // Note: it works, but we rely on the fact that the same thread will run the continuation.
                // It's required for the forking workload, but it can be changed for cases when normal task
                // awaits ParallelTask (with frame protection)
                _parallelResult = new ParallelTaskResult<T>(Assertion.BadAwaitExceptionDispatchInfo);
                
            }

            ParallelContext.RestoreNoVerification(_contextBeforeAwait); //TODO: is it legal in case of BadAwaitExceptionDispatchInfo? Don't we get some unrecoverable side effects
            _action.Invoke();
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