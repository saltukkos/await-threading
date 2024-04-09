// MIT License
// Copyright (c) 2023 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;

namespace AwaitThreading.Core;

public static class ParallelTaskMethodBuilderImpl
{
    public static void ParallelOnCompleted<TAwaiter, TStateMachine>(
        ref TAwaiter parallelAwaiter,
        TStateMachine stateMachine)
        where TAwaiter : IParallelNotifyCompletion
        where TStateMachine : IAsyncStateMachine
    {
        var stateMachineLocal = MakeCopy(stateMachine);
        parallelAwaiter.ParallelOnCompleted(() => { MakeCopy(stateMachineLocal).MoveNext(); });
    }

    public static void OnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, TStateMachine stateMachine)
        where TAwaiter : INotifyCompletion
        where TStateMachine : IAsyncStateMachine
    {
        var executionContext = ExecutionContext.Capture();

        if (executionContext is null)
        {
            awaiter.OnCompleted(() => { stateMachine.MoveNext(); });
        }
        else
        {
            awaiter.OnCompleted(() =>
            {
                ExecutionContext.Restore(executionContext);
                MakeCopy(stateMachine).MoveNext();
            });
        }
    }

    public static void UnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, TStateMachine stateMachine)
        where TAwaiter : ICriticalNotifyCompletion
        where TStateMachine : IAsyncStateMachine
    {
        var executionContext = ExecutionContext.Capture();

        if (executionContext is null)
        {
            awaiter.UnsafeOnCompleted(() => { stateMachine.MoveNext(); });
        }
        else
        {
            awaiter.UnsafeOnCompleted(() =>
            {
                ExecutionContext.Restore(executionContext);
                MakeCopy(stateMachine).MoveNext();
            });
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static TStateMachine MakeCopy<TStateMachine>(TStateMachine stateMachine)
        where TStateMachine : IAsyncStateMachine
    {
        if (typeof(TStateMachine).IsValueType)
        {
            // in release mode this method sould be inlined with no copy overhead at all
            return stateMachine;
        }

        return (TStateMachine) stateMachine.Copy();
    }
}