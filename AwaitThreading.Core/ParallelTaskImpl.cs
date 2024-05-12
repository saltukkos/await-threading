// MIT License
// Copyright (c) 2023 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
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
    
#if FEATTURE_DEDICATED_SYNCRESULT
    private ParallelTaskResult<T>? _syncResult;

#if FEATURE_TASKIMPL_NORESULT
    private volatile int _resCount = 0;
#elif FEATURE_TASKIMPL_ASYNCLOCAL
    private AsyncLocal<ParallelTaskResult<T>>? _results;
#elif FEATURE_TASKIMPL_QUEUE
    private ConcurrentQueue<ParallelTaskResult<T>>? _results;
#else
    private ConcurrentStack<ParallelTaskResult<T>>? _results;
#endif

#else

#if FEATURE_TASKIMPL_NORESULT
    private volatile int _resCount = 0;
#elif FEATURE_TASKIMPL_ASYNCLOCAL
    private readonly AsyncLocal<ParallelTaskResult<T>> _result = new();
#elif FEATURE_TASKIMPL_QUEUE
    private readonly ConcurrentQueue<ParallelTaskResult<T>> _results = new();
#else
    private readonly ConcurrentStack<ParallelTaskResult<T>> _results = new();
#endif

#endif

    private volatile Action? _continuation;

    private bool _requireContinuationToBeSetBeforeResult;

    /// <summary>
    /// Indicates whether task requires SetContinuation call before SetResult happens.
    /// We need this guarantee when forking so every thread will be able to run continuation 
    /// </summary>
    public bool RequireContinuationToBeSetBeforeResult
    {
        get => _requireContinuationToBeSetBeforeResult;
        internal set
        {
#if FEATTURE_DEDICATED_SYNCRESULT
            _results ??= new();
#endif
            _requireContinuationToBeSetBeforeResult = value;
        }
    }

    public void SetResult(ParallelTaskResult<T> result)
    {
        // normal control flow: if the continuation is here, run it. If no - save result to run on continuation set
        if (!RequireContinuationToBeSetBeforeResult)
        {
#if FEATTURE_DEDICATED_SYNCRESULT
            _syncResult = result;
#else
            SetParallelResult(result);
#endif
            var oldValue = Interlocked.CompareExchange(ref _continuation, ParallelTaskStaticData.TaskFinishedFlag, null);
            if (oldValue != null)
            {
                Debug.Assert(oldValue != ParallelTaskStaticData.TaskFinishedFlag);
                oldValue.Invoke();
            }
            
            return;
        }

        SetParallelResult(result);

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

    /// <summary>
    /// Achtung! This method is not pure and has to be called only once per thread. Additinal call will lead to deadlock
    /// </summary>
    internal ParallelTaskResult<T> GetResult()
    {
        if (RequireContinuationToBeSetBeforeResult)
        {
            if (_continuation is null) Assertion.ThrowInvalidDirectGetResultCall();
            return GetParallelResult();
        }
#if FEATTURE_DEDICATED_SYNCRESULT
        return _syncResult!.Value;
#else
        return GetParallelResult();
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ParallelTaskResult<T> GetParallelResult()
    {
#if FEATURE_TASKIMPL_NORESULT
        var decrementedValue = Interlocked.Decrement(ref _resCount);
        if (decrementedValue < 0)
        {
            IllegalWaitHappened();
        }

        return new ParallelTaskResult<T>(default(T)!);
#elif FEATURE_TASKIMPL_ASYNCLOCAL
        return _results!.Value;
#elif FEATURE_TASKIMPL_QUEUE
        if (!_results.TryDequeue(out var result))
        {
            IllegalWaitHappened();
        }

        return result;
#else
        if (!_results.TryPop(out var result))
        {
            IllegalWaitHappened();
        }

        return result;
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetParallelResult(ParallelTaskResult<T> result)
    {
#if FEATURE_TASKIMPL_NORESULT
        Interlocked.Increment(ref _resCount);
#elif FEATURE_TASKIMPL_ASYNCLOCAL
        _results.Value = result;
#elif FEATURE_TASKIMPL_QUEUE
        _results.Enqueue(result);
#else
        _results.Push(result);
#endif
    }

    private static void IllegalWaitHappened()
    {
        Debug.Fail("Illegal wait");
        Thread.Sleep(10_000); //TODO: remove after benchmarks
        Assertion.ThrowBadAwait();
    }

    public bool IsCompleted
    {
        get
        {
            if (RequireContinuationToBeSetBeforeResult)
            {
                return false;
            }

#if FEATTURE_DEDICATED_SYNCRESULT
            return _syncResult.HasValue;
#elif FEATURE_TASKIMPL_NORESULT
            return _resCount > 0;
#elif FEATURE_TASKIMPL_ASYNCLOCAL
            IllegalWaitHappened();
            return true;
#elif FEATURE_TASKIMPL_QUEUE
            return !_results.IsEmpty;
#else
            return !_results.IsEmpty;
#endif
        }
    }

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
                Debug.Assert(RequireContinuationToBeSetBeforeResult);
                GetParallelResult();
                SetParallelResult(new ParallelTaskResult<T>(Assertion.BadAwaitExceptionDispatchInfo));
            }

#if FEATTURE_DEDICATED_SYNCRESULT
            RequireContinuationToBeSetBeforeResult = false;
            _syncResult = GetParallelResult();
#endif

            continuation.Invoke();
        });
    }

    
    public void UnsafeOnCompleted(Action continuation)
    {
        // TODO: do we need a proper implementation?
        OnCompleted(continuation);
    }
}