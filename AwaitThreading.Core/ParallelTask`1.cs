//MIT License
//Copyright (c) 2023 Saltuk Konstantin
//See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;

namespace AwaitThreading.Core;

[AsyncMethodBuilder(typeof(ParallelTaskMethodBuilder<>))]
public readonly struct ParallelTask<T>
{
    internal readonly ParallelTaskImpl<T> Implementation;

    public ParallelTask()
    {
        Implementation = new ParallelTaskImpl<T>();
    }

    internal void SetResult(T result) =>
        Implementation.SetResult(new ParallelTaskResult<T>(result));

    internal void SetException(Exception e) =>
        Implementation.SetResult(new ParallelTaskResult<T>(ExceptionDispatchInfo.Capture(e)));

    internal void SetStateMachine<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine =>
        Implementation.SetStateMachine(ref stateMachine); 

    public ParallelTaskAwaiter<T> GetAwaiter() => new(Implementation);
}