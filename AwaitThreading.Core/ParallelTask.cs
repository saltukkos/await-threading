// MIT License
// Copyright (c) 2023 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;

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

    internal void SetException(Exception e) =>
        _implementation.SetResult(new ParallelTaskResult<Unit>(ExceptionDispatchInfo.Capture(e)));

    public ParallelTaskAwaiter GetAwaiter() => new(_implementation);
}