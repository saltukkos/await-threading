// MIT License
// Copyright (c) 2023 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;

namespace AwaitThreading.Core;

[AsyncMethodBuilder(typeof(ParallelTaskMethodBuilder))]
public readonly struct ParallelTask
{
    internal readonly ParallelTaskImpl<Unit> Implementation;

    public ParallelTask()
    {
        Implementation = new ParallelTaskImpl<Unit>();
    }

    internal void SetResult() => Implementation.SetResult(default);

    internal void SetException(Exception e) =>
        Implementation.SetResult(new ParallelTaskResult<Unit>(ExceptionDispatchInfo.Capture(e)));

    internal void SetStateMachine<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
        => Implementation.SetStateMachine(ref stateMachine);

    public ParallelTaskAwaiter GetAwaiter() => new(Implementation);
}