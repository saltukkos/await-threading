// MIT License
// Copyright (c) 2023 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;

namespace AwaitThreading.Core;

[AsyncMethodBuilder(typeof(ParallelTaskMethodBuilder))]
public readonly struct ParallelTask
{
    private readonly ParallelTaskImpl<Unit> _implementation;

    public ParallelTask()
    {
        _implementation = new ParallelTaskImpl<Unit>();
    }

    internal void MarkAsRequireContinuationToBeSetBeforeResult()
    {
        _implementation.RequireContinuationToBeSetBeforeResult = true;
    }

    internal void SetResult() => _implementation.SetResult(default);

    /// <summary>
    /// Achtung! This method is not pure and has to be called only once per thread. Additinal call will lead to deadlock
    /// </summary>
    public void GetResult() => _implementation.GetResult();

    public ParallelTaskAwaiter GetAwaiter() => new(_implementation);

    public void SetContinuation(Action continuation) => _implementation.SetContinuation(continuation);
}