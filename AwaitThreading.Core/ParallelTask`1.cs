//MIT License
//Copyright (c) 2023 Saltuk Konstantin
//See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;

namespace AwaitThreading.Core;

[AsyncMethodBuilder(typeof(ParallelTaskMethodBuilder<>))]
public readonly struct ParallelTask<T>
{
    private readonly ParallelTaskImpl<T> _implementation;

    public ParallelTask()
    {
        _implementation = new ParallelTaskImpl<T>();
    }

    internal void MarkAsRequireContinuationToBeSetBeforeResult()
    {
        _implementation.RequireContinuationToBeSetBeforeResult = true;
    }

    internal void SetResult(T result) => _implementation.SetResult(result);

    /// <summary>
    /// Achtung! This method is not pure and has to be called only once per thread. Additinal call will lead to deadlock
    /// </summary>
    public T GetResult() => _implementation.GetResult();

    public ParallelTaskAwaiter<T> GetAwaiter() => new(_implementation);

    public void SetContinuation(Action continuation) => _implementation.SetContinuation(continuation);
}