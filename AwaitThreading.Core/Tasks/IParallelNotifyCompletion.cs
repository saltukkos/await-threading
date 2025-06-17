// MIT License
// Copyright (c) 2023 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;

namespace AwaitThreading.Core.Tasks;

public interface IParallelNotifyCompletion
{
    /// <summary>
    /// Same as AwaitUnsafeOnCompleted, but with ParallelTask scenarios support:
    /// Implementations are required to restore the execution context when invoking stateMachine.MoveNext
    /// </summary>
    void ParallelOnCompleted<TStateMachine>(TStateMachine stateMachine)
        where TStateMachine : IAsyncStateMachine;
}