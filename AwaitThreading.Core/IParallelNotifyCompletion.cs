// MIT License
// Copyright (c) 2023 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;

namespace AwaitThreading.Core;

public interface IParallelNotifyCompletion
{
    void ParallelOnCompleted<TStateMachine>(TStateMachine stateMachine)
        where TStateMachine : IAsyncStateMachine;
}