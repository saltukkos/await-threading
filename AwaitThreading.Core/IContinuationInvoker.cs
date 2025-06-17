// MIT License
// Copyright (c) 2024 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;

namespace AwaitThreading.Core;

internal interface IContinuationInvoker
{
    void Invoke();
}

internal sealed class ParallelContinuationInvoker<TStateMachine> : IContinuationInvoker
    where TStateMachine : IAsyncStateMachine
{
    private readonly TStateMachine _stateMachine;

    public ParallelContinuationInvoker(TStateMachine stateMachine)
    {
        _stateMachine = stateMachine.MakeCopy();
    }

    public void Invoke()
    {
        _stateMachine.MakeCopy().MoveNext();
    }
}

// internal sealed class TaskFinishedMarker : IContinuationInvoker
// {
//     public static readonly TaskFinishedMarker Instance = new();
//
//     private TaskFinishedMarker()
//     {
//     }
//
//     public void Invoke()
//     {
//     }
// }
