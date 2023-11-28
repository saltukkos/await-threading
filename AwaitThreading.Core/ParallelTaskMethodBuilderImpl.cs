// MIT License
// Copyright (c) 2023 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;

namespace AwaitThreading.Core;

public static class ParallelTaskMethodBuilderImpl
{
    public static void ParallelOnCompleted<TStateMachine>(TStateMachine stateMachine,
        IParallelNotifyCompletion parallelAwaiter) where TStateMachine : IAsyncStateMachine
    {
        var stateMachineLocal = MakeCopy(stateMachine);
        parallelAwaiter.ParallelOnCompleted(() => { MakeCopy(stateMachineLocal).MoveNext(); });
    }

    public static void OnCompleted<TAwaiter, TStateMachine>(TAwaiter awaiter, TStateMachine stateMachine)
        where TAwaiter : INotifyCompletion where TStateMachine : IAsyncStateMachine
    {
        var stateMachineLocal = stateMachine;
        var executionContext = ExecutionContext.Capture();

        if (executionContext is null)
        {
            awaiter.OnCompleted(() => { stateMachineLocal.MoveNext(); });
        }
        else
        {
            awaiter.OnCompleted(() =>
            {
                ExecutionContext.Restore(executionContext);
                MakeCopy(stateMachineLocal).MoveNext();
            });
        }
    }

    public static void UnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : ICriticalNotifyCompletion
        where TStateMachine : IAsyncStateMachine
    {
        var stateMachineLocal = stateMachine;
        var executionContext = ExecutionContext.Capture();

        if (executionContext is null)
        {
            awaiter.UnsafeOnCompleted(() => { stateMachineLocal.MoveNext(); });
        }
        else
        {
            awaiter.UnsafeOnCompleted(() =>
            {
                ExecutionContext.Restore(executionContext);
                MakeCopy(stateMachineLocal).MoveNext();
            });
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static TStateMachine MakeCopy<TStateMachine>(TStateMachine stateMachine)
        where TStateMachine : IAsyncStateMachine
    {
        if (typeof(TStateMachine).IsValueType)
        {
            return stateMachine;
        }

        return (TStateMachine) stateMachine.Copy();
    }
}