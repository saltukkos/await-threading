//MIT License
//Copyright (c) 2023 Saltuk Konstantin
//See the LICENSE file in the project root for more information.

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace AwaitThreading.Core;

[AsyncMethodBuilder(typeof(ParallelTaskMethodBuilder<>))]
public sealed class ParallelTask<T>
{
    //TODO disposing?
    private readonly BlockingCollection<T> _results = new();
    private readonly ManualResetEvent _waitHandle = new(false);
    private Action? _continuation;

    public bool ReturnSynchronously { get; internal set; } = true;
    
    internal void SetResult(T result)
    {
        _results.Add(result);
        
        Console.Out.WriteLine($"Start waiting handle for {_waitHandle.GetHashCode()} thread {Thread.CurrentThread.ManagedThreadId}");
        if (ReturnSynchronously)
        {
            return;
        }

        _waitHandle.WaitOne();
        _continuation!.Invoke();
    }

    public T GetResult()
    {
        return _results.Take();
    }

    public ParallelTaskAwaiter<T> GetAwaiter() => new(this);

    public void SetContinuation(Action continuation)
    {
        if (ReturnSynchronously)
        {
            throw new InvalidOperationException("Expect caller to run synchronously");
        }
        
        _continuation = continuation;
        _waitHandle.Set();
        Console.Out.WriteLine($"Set wait handle for {_waitHandle.GetHashCode()} thread {Thread.CurrentThread.ManagedThreadId}");
    }
}