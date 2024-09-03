// MIT License
// Copyright (c) 2023 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;

namespace AwaitThreading.Core;

public static class ParallelTaskMethodBuilderImpl
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void AwaitParallelOnCompleted<TAwaiter, TStateMachine>(
        ref TAwaiter parallelAwaiter,
        TStateMachine stateMachine)
        where TAwaiter : IParallelNotifyCompletion
        where TStateMachine : IAsyncStateMachine
    {
        // TODO: we need to restore ExecutionContext and reimplement ParallelLazyAsyncEnumerator
        parallelAwaiter.ParallelOnCompleted(stateMachine);
    }

    public static void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, TStateMachine stateMachine)
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
                ExecutionContext.Restore(executionContext); //TODO: custom closure class instead of lambda
                stateMachine.MoveNext();
            });
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, TStateMachine stateMachine)
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
                stateMachine.MoveNext();
            });
        }
    }
}