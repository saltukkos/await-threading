// MIT License
// Copyright (c) 2023 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace AwaitThreading.Core;

[AsyncMethodBuilder(typeof(ParallelTaskMethodBuilder))]
public sealed class ParallelTask //TODO copy-paste?
{
    //TODO disposing?
    private readonly BlockingCollection<Unit> _results = new();
    private readonly ManualResetEvent _waitHandle = new(false);
    private Action? _continuation;

    public bool ReturnSynchronously { get; internal set; }
    
    internal void SetResult()
    {
        _results.Add(default);
        
        Console.Out.WriteLine($"Start waiting handle for {_waitHandle.GetHashCode()} thread {Thread.CurrentThread.ManagedThreadId}");
        if (ReturnSynchronously)
        {
            return;
        }

        _waitHandle.WaitOne(); // JoiningTask всё ещё приходит сюда, хотя как бы синхронно. Делать новое проперти в awaiter'е?
        _continuation!.Invoke();
    }

    public void GetResult()
    {
        _results.Take();
    }

    public ParallelTaskAwaiter GetAwaiter() => new(this);

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