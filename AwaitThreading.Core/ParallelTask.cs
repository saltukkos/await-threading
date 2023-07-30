// MIT License
// Copyright (c) 2023 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace AwaitThreading.Core;

[AsyncMethodBuilder(typeof(ParallelTaskMethodBuilder))]
public sealed class ParallelTask //TODO copy-paste?
{
    private Action? _continuation;
    private readonly BlockingCollection<Unit> _results = new();

    internal void SetResult()
    {
        _results.Add(default);
        _continuation?.Invoke();
    }

    public void GetResult()
    {
        _results.Take();
    }

    public ParallelTaskAwaiter GetAwaiter() => new(this);

    public void SetContinuation(Action continuation)
    {
        _continuation = continuation;
        if (_results.Count > 0) //TODO is it even possible?
        {
            continuation.Invoke();
        }
    }
}