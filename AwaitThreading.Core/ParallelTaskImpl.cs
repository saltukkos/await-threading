// MIT License
// Copyright (c) 2023 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace AwaitThreading.Core;

internal sealed class ParallelTaskImpl<T>
{
    //TODO disposing?
    private readonly BlockingCollection<T> _results = new();
    private Action? _continuation;

    /// <summary>
    /// Indicates whether task requires SetContinuation call before SetResult happens.
    /// We need this garuantee when forking so every thread will be able to run continuation 
    /// </summary>
    public bool RequireContinuationToBeSetBeforeResult { get; internal set; }
    
    public void SetResult(T result)
    {
        RetrieveContinuationIfNeed()?.Invoke();

        Action? RetrieveContinuationIfNeed()
        {
            lock (this)
            {
                Console.Out.WriteLine($"{Tim.Er} setting result to task {this.GetHashCode() % 100}");

                _results.Add(result);
                // normal control flow: if the continuation is here, run it. If no - save result to run on continuation set
                if (!RequireContinuationToBeSetBeforeResult)
                {
                    Console.Out.WriteLine($"{Tim.Er} calling continuation synchronously for task {this.GetHashCode() % 100}");

                    // TODO have one result instead of collection in this case?
                    return _continuation;
                }


                // special control flow: we need continuation to be already set. If no - we will wait until it's done
                while (_continuation is null)
                {
                    Console.Out.WriteLine($"{Tim.Er} waiting for continuation on task {this.GetHashCode() % 100}");
                    Monitor.Wait(this);
                }

                Console.Out.WriteLine($"{Tim.Er} calling continuation asynchronously for task {this.GetHashCode() % 100}");
                return _continuation;
            }
        }
    }

    /// <summary>
    /// Achtung! This method is not pure and has to be called only once per thread. Additinal call will lead to deadlock
    /// </summary>
    public T GetResult()
    {
        return _results.Take();
    }

    public void SetContinuation(Action continuation)
    {
        RetrieveContinuationIfNeed()?.Invoke();

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
}