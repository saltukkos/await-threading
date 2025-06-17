// MIT License
// Copyright (c) 2023 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;
using AwaitThreading.Core.Context;

namespace AwaitThreading.Core.Tasks;

internal static class ParallelTaskMethodBuilderImpl
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AwaitOnCompleted<TAwaiter, TStateMachine>(
        ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : INotifyCompletion
        where TStateMachine : IAsyncStateMachine
    {
        if (awaiter is IParallelNotifyCompletion parallelAwaiter)
        {
            AwaitParallelOnCompletedInternal(ref parallelAwaiter, stateMachine);
        }
        else
        {
            AwaitOnCompletedInternal(ref awaiter, stateMachine);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(
        ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : ICriticalNotifyCompletion
        where TStateMachine : IAsyncStateMachine
    {
        if (awaiter is IParallelNotifyCompletion parallelAwaiter)
        {
            AwaitParallelOnCompletedInternal(ref parallelAwaiter, stateMachine);
        }
        else
        {
            AwaitUnsafeOnCompletedInternal(ref awaiter, stateMachine);
        }
    }

    private static void AwaitParallelOnCompletedInternal<TAwaiter, TStateMachine>(
        ref TAwaiter parallelAwaiter,
        TStateMachine stateMachine)
        where TAwaiter : IParallelNotifyCompletion
        where TStateMachine : IAsyncStateMachine
    {
        parallelAwaiter.ParallelOnCompleted(stateMachine);
    }

    private static void AwaitOnCompletedInternal<TAwaiter, TStateMachine>(
        ref TAwaiter awaiter,
        TStateMachine stateMachine)
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

    private static void AwaitUnsafeOnCompletedInternal<TAwaiter, TStateMachine>(
        ref TAwaiter awaiter,
        TStateMachine stateMachine)
        where TAwaiter : ICriticalNotifyCompletion
        where TStateMachine : IAsyncStateMachine
    {
        var executionContext = ExecutionContext.Capture();
        var parallelContext = ParallelContextStorage.CaptureAndClear();
    
        if (executionContext is null)
        {
            awaiter.UnsafeOnCompleted(() =>
            {
                ParallelContextStorage.Restore(parallelContext);
                stateMachine.MoveNext();
                ParallelContextStorage.ClearButNotExpected();
            });
        }
        else
        {
            awaiter.UnsafeOnCompleted(() =>
            {
                ExecutionContext.Restore(executionContext);
                ParallelContextStorage.Restore(parallelContext);
                stateMachine.MoveNext();
                ParallelContextStorage.ClearButNotExpected();
            });
        }
    }
}