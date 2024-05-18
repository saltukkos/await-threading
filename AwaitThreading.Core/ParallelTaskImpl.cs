// MIT License
// Copyright (c) 2023 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace AwaitThreading.Core;

internal sealed class ParallelTaskImpl<T>
{
    [ThreadStatic]
    private static ParallelTaskResult<T>? _parallelResult;

    private ParallelTaskResult<T>? _syncResult;

    private volatile IContinuationInvoker? _continuation;

    /// <summary>
    /// Indicates whether task requires SetContinuation call before SetResult happens.
    /// We need this guarantee when forking so every thread will be able to run continuation 
    /// </summary>
    public bool RequireContinuationToBeSetBeforeResult { get; internal set; }

    public void SetResult(ParallelTaskResult<T> result)
    {
        // normal control flow: if the continuation is here, run it. If no - save result to run on continuation set
        if (!RequireContinuationToBeSetBeforeResult)
        {
            _syncResult = result;

            var oldValue = Interlocked.CompareExchange(ref _continuation, TaskFinishedMarker.Instance, null);
            if (oldValue != null)
            {
                Debug.Assert(!ReferenceEquals(oldValue, TaskFinishedMarker.Instance));
                oldValue.Invoke();
            }
            
            return;
        }

        _parallelResult = result;

        // special control flow: we need continuation to be already set. If no, we will wait until it's done
        var continuation = _continuation;
        if (continuation == null)
        {
            var spinWait = new SpinWait();
            do
            {
                spinWait.SpinOnce();
                continuation = _continuation;
            } while (continuation is null);
        }

        continuation.Invoke();
    }

    internal ParallelTaskResult<T> GetResult()
    {
        if (RequireContinuationToBeSetBeforeResult)
        {
            if (_continuation is null || !_parallelResult.HasValue)
            {
                Assertion.ThrowInvalidDirectGetResultCall();
            }

            return _parallelResult.Value;
        }

        if (!_syncResult.HasValue)
        {
            Assertion.ThrowInvalidDirectGetResultCall();
        }

        return _syncResult.Value;
    }

    public bool IsCompleted => !RequireContinuationToBeSetBeforeResult && _syncResult.HasValue;

    public void ParallelOnCompleted<TStateMachine>(TStateMachine stateMachine) 
        where TStateMachine : IAsyncStateMachine
    {
        OnCompletedInternal(new ParallelContinuationInvoker<TStateMachine>(stateMachine));
    }

    public void OnCompleted(Action continuation)
    {
        OnCompletedInternal(new RegularContinuationInvoker(continuation));
    }

    public void UnsafeOnCompleted(Action continuation)
    {
        // TODO: do we need a proper implementation?
        OnCompleted(continuation);
    }

    private void OnCompletedInternal(IContinuationInvoker continuationInvoker)
    {
        if (RequireContinuationToBeSetBeforeResult)
        {
            // in this control flow continuation should be set before 'SetResult' finishes. So, another thread could
            // wait for this continuation while spinning in 'SetResult', we only write to the volatile field.
            _continuation = continuationInvoker;
            return;
        }
        
        var oldContinuation = Interlocked.CompareExchange(ref _continuation, continuationInvoker, null);
        if (oldContinuation == null)
        {
            return;
        }

        Debug.Assert(ReferenceEquals(oldContinuation, TaskFinishedMarker.Instance));
        continuationInvoker.Invoke();
    }

    private sealed class RegularContinuationInvoker : IContinuationInvoker
    {
        private readonly Action _action;
        private readonly ParallelFrame? _frameBeforeAwait;
        public RegularContinuationInvoker(Action action)
        {
            _frameBeforeAwait = ParallelContext.GetCurrentFrameSafe();
            _action = action;
        }

        public void Invoke()
        {
            var currentFrameAfterAwait = ParallelContext.GetCurrentFrameSafe();
            if (currentFrameAfterAwait?.ForkIdentity != _frameBeforeAwait?.ForkIdentity)
            {
                // note: we can be here only after parallel operations,
                // so RequireContinuationToBeSetBeforeResult is true, and we can use the `_parallelResult`

                _parallelResult = new ParallelTaskResult<T>(Assertion.BadAwaitExceptionDispatchInfo);
            }

            _action.Invoke();
        }
    }
}