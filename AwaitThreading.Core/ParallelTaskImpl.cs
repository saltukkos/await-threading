// MIT License
// Copyright (c) 2023 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.ExceptionServices;

namespace AwaitThreading.Core;

public readonly struct ParallelTaskResult<T>
{
    public ParallelTaskResult(T result)
    {
        Result = result;
        ExceptionDispatchInfo = null;
    }

    public ParallelTaskResult(ExceptionDispatchInfo exceptionDispatchInfo)
    {
        ExceptionDispatchInfo = exceptionDispatchInfo;
        Result = default;
    }

    [MemberNotNullWhen(true, nameof(Result))]
    [MemberNotNullWhen(false, nameof(ExceptionDispatchInfo))]
    public bool HasResult => ExceptionDispatchInfo is null;

    public readonly T? Result;

    public readonly ExceptionDispatchInfo? ExceptionDispatchInfo;
}

internal static class ParallelTaskStaticData
{
    public static readonly Action TaskFinishedFlag = () => { };
}

internal sealed class ParallelTaskImpl<T>
{
    [ThreadStatic]
    private static ParallelTaskResult<T>? _parallelResult;

    private ParallelTaskResult<T>? _syncResult;

    private volatile Action? _continuation;

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

            var oldValue = Interlocked.CompareExchange(ref _continuation, ParallelTaskStaticData.TaskFinishedFlag, null);
            if (oldValue != null)
            {
                Debug.Assert(oldValue != ParallelTaskStaticData.TaskFinishedFlag);
                oldValue.Invoke();
            }
            
            return;
        }

        _parallelResult = result;

        // special control flow: we need continuation to be already set. If no, we will wait until it's done
        var continuation = _continuation;
        if (continuation != null)
        {
            continuation.Invoke();
            return;
        }

        var spinWait = new SpinWait();
        do
        {
            spinWait.SpinOnce();
            continuation = _continuation;
        }
        while (continuation is null);
        
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

    public void ParallelOnCompleted(Action continuation)
    {
        if (RequireContinuationToBeSetBeforeResult)
        {
            _continuation = continuation;
            return;
        }

        var oldContinuation = Interlocked.CompareExchange(ref _continuation, continuation, null);
        if (oldContinuation == null)
        {
            return;
        }

        Debug.Assert(oldContinuation == ParallelTaskStaticData.TaskFinishedFlag);
        continuation.Invoke();
    }

    public void OnCompleted(Action continuation)
    {
        var currentFrameBeforeAwait = ParallelContext.GetCurrentFrameSafe();
        ParallelOnCompleted(() =>
        {
            var currentFrameAfterAwait = ParallelContext.GetCurrentFrameSafe();
            if (currentFrameAfterAwait?.ForkIdentity != currentFrameBeforeAwait?.ForkIdentity)
            {
                //note: there is no other way for us to be here, only after parallel operations
                Debug.Assert(RequireContinuationToBeSetBeforeResult);

                _parallelResult = new ParallelTaskResult<T>(Assertion.BadAwaitExceptionDispatchInfo);
            }

            continuation.Invoke();
        });
    }

    public void UnsafeOnCompleted(Action continuation)
    {
        // TODO: do we need a proper implementation?
        OnCompleted(continuation);
    }
}