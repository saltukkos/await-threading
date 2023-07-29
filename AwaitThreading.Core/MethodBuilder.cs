//MIT License
//Copyright (c) 2023 Saltuk Konstantin
//See the LICENSE file in the project root for more information.

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace AwaitThreading.Core;

[AsyncMethodBuilder(typeof(ParallelTaskMethodBuilder<>))]
public sealed class ParallelTask<T>
{
    private Action? _continuation;
    private readonly BlockingCollection<T> _results = new();

    internal void SetResult(T result)
    {
        _results.Add(result);
        _continuation?.Invoke();
    }

    public T GetResult()
    {
        return _results.Take();
    }

    public ParallelTaskAwaiter<T> GetAwaiter() => new(this);

    public void SetContinuation(Action continuation)
    {
        _continuation = continuation;
        if (_results.Count > 0) //TODO is it even possible?
        {
            continuation.Invoke();
        }
    }
}