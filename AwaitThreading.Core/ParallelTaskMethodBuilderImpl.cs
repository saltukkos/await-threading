// MIT License
// Copyright (c) 2023 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;

namespace AwaitThreading.Core;

internal static class ParallelTaskMethodBuilderImpl
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AwaitOnCompleted<TAwaiter, TStateMachine, TResult>(
        ref TAwaiter awaiter, ref TStateMachine stateMachine, ref ParallelTaskImpl<TResult>? taskImpl)
        where TAwaiter : INotifyCompletion
        where TStateMachine : IAsyncStateMachine
    {
        if (awaiter is IParallelNotifyCompletion parallelAwaiter)
        {
            if (parallelAwaiter.RequireContinuationToBeSetBeforeResult)
            {
                taskImpl ??= new ParallelTaskImpl<TResult>();
                taskImpl.RequireContinuationToBeSetBeforeResult = true;
            }

            AwaitParallelOnCompletedInternal(ref parallelAwaiter, stateMachine);
        }
        else
        {
            AwaitOnCompletedInternal(ref awaiter, stateMachine);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine, TResult>(
        ref TAwaiter awaiter, ref TStateMachine stateMachine, ref ParallelTaskImpl<TResult>? taskImpl)
        where TAwaiter : ICriticalNotifyCompletion
        where TStateMachine : IAsyncStateMachine
    {
        if (awaiter is IParallelNotifyCompletion parallelAwaiter)
        {
            if (parallelAwaiter.RequireContinuationToBeSetBeforeResult)
            {
                taskImpl ??= new ParallelTaskImpl<TResult>();
                taskImpl.RequireContinuationToBeSetBeforeResult = true;
            }

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
        // TODO: we need to restore ExecutionContext and reimplement ParallelLazyAsyncEnumerator
        parallelAwaiter.ParallelOnCompleted(stateMachine);
    }

    private static void AwaitOnCompletedInternal<TAwaiter, TStateMachine>(ref TAwaiter awaiter, TStateMachine stateMachine)
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
    private static void AwaitUnsafeOnCompletedInternal<TAwaiter, TStateMachine>(ref TAwaiter awaiter, TStateMachine stateMachine)
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