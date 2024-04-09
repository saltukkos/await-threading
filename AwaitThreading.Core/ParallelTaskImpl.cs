// MIT License
// Copyright (c) 2023 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

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

internal sealed class ParallelTaskImpl<T>
{
    private readonly BlockingQueue<ParallelTaskResult<T>> _results = new();
    private Action? _continuation;

    /// <summary>
    /// Indicates whether task requires SetContinuation call before SetResult happens.
    /// We need this garuantee when forking so every thread will be able to run continuation 
    /// </summary>
    public bool RequireContinuationToBeSetBeforeResult { get; internal set; }

    public void SetResult(ParallelTaskResult<T> result)
    {
        RetrieveContinuationIfNeed()?.Invoke();
        return;

        Action? RetrieveContinuationIfNeed()
        {
            lock (this)
            {
                Logger.Log($"setting result to task {this.GetHashCode() % 100}");

                _results.Add(result);

                // normal control flow: if the continuation is here, run it. If no - save result to run on continuation set
                if (!RequireContinuationToBeSetBeforeResult)
                {
                    Logger.Log($"calling continuation synchronously for task {this.GetHashCode() % 100}");

                    // TODO have one result instead of collection in this case?
                    return _continuation;
                }

                // special control flow: we need continuation to be already set. If no, we will wait until it's done
                while (_continuation is null)
                {
                    Logger.Log($"waiting for continuation on task {this.GetHashCode() % 100}");
                    Monitor.Wait(this);
                }

                Logger.Log($"calling continuation asynchronously for task {this.GetHashCode() % 100}");
                return _continuation;
            }
        }
    }

    /// <summary>
    /// Achtung! This method is not pure and has to be called only once per thread. Additinal call will lead to deadlock
    /// </summary>
    public ParallelTaskResult<T> GetResult()
    {
        return _results.Take();
    }

    public bool IsCompleted => !RequireContinuationToBeSetBeforeResult && _results.Count > 0;

    public void ParallelOnCompleted(Action continuation)
    {
        RetrieveContinuationIfNeed()?.Invoke();
        return;

        Action? RetrieveContinuationIfNeed()
        {
            lock (this)
            {
                _continuation = continuation;
                // normal control flow: if the result is here, run continuation. If no - save continuation to run on result set
                if (!RequireContinuationToBeSetBeforeResult)
                {
                    if (_results.Count > 0)
                    {
                        return continuation;
                    }

                    return null;
                }

                // special control flow: we need continuation to be set before result. Signal the waiters to run
                Monitor.PulseAll(this);
                return null;
            }
        }
    }

    public void OnCompleted(Action continuation)
    {
        var currentFrameBeforeAwait = ParallelContext.GetCurrentFrameSafe();
        ParallelOnCompleted(() =>
        {
            var currentFrameAfterAwait = ParallelContext.GetCurrentFrameSafe();
            if (currentFrameAfterAwait?.ForkIdentity != currentFrameBeforeAwait?.ForkIdentity)
            {
                _results.Take();
                _results.Add(new ParallelTaskResult<T>(Assertion.BadAwaitExceptionDispatchInfo));
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