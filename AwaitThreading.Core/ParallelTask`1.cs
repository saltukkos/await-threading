//MIT License
//Copyright (c) 2023 Saltuk Konstantin
//See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;

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

    internal void SetResult(T result) => _implementation.SetResult(new ParallelTaskResult<T>(result));
    internal void SetException(Exception e) =>
        _implementation.SetResult(new ParallelTaskResult<T>(ExceptionDispatchInfo.Capture(e)));

    /// <summary>
    /// Achtung! This method is not pure and has to be called only once per thread. Additional call will lead to deadlock
    /// </summary>
    public ParallelTaskResult<T> GetResult() => _implementation.GetResult();

    public ParallelTaskAwaiter<T> GetAwaiter() => new(_implementation);
}