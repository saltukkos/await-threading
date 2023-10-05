// MIT License
// Copyright (c) 2023 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace AwaitThreading.Core;

public readonly struct ParallelTaskMethodBuilder
{
    public ParallelTaskMethodBuilder()
    {
    }

    public static ParallelTaskMethodBuilder Create() => new();

    public ParallelTask Task { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; } = new();

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
    {
        stateMachine.MoveNext();
    }

    public void SetStateMachine(IAsyncStateMachine stateMachine)
    {
    }

    public void AwaitOnCompleted<TAwaiter, TStateMachine>(
        ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : INotifyCompletion
        where TStateMachine : IAsyncStateMachine
    {
        if (awaiter is IParallelNotifyCompletion parallelAwaiter)
        {
            var stateMachineLocal = MakeCopy(stateMachine);
            parallelAwaiter.ParallelOnCompleted(() =>
            {
                MakeCopy(stateMachineLocal).MoveNext();
            });
        }
        else
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
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(
        ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : ICriticalNotifyCompletion
        where TStateMachine : IAsyncStateMachine
    {
        if (awaiter is IParallelNotifyCompletion parallelAwaiter)
        {
            var stateMachineLocal = MakeCopy(stateMachine);
            parallelAwaiter.ParallelOnCompleted(() =>
            {
                MakeCopy(stateMachineLocal).MoveNext();
            });
        }
        else
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
    }

    public void SetResult() => Task.SetResult();

    public void SetException(Exception exception)
    {
        Debugger.Break();
        throw new NotImplementedException();
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