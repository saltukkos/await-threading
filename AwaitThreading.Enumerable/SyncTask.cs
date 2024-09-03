// MIT License
// Copyright (c) 2024 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;

namespace AwaitThreading.Enumerable;

/// <summary>
/// Even more optimized version of ValueTask that supports only sync returns. Should be free in terms of overhead after inlining
/// </summary>
public readonly struct SyncTask<T> : INotifyCompletion
{
    private readonly T _result;

    public SyncTask(T result)
    {
        _result = result;
    }

    public SyncTask<T> GetAwaiter() => this;

    public bool IsCompleted
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetResult() => _result;

    public void OnCompleted(Action continuation)
    {
    }
}