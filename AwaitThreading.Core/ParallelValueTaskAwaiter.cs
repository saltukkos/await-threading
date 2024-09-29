// MIT License
// Copyright (c) 2024 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;

namespace AwaitThreading.Core;

public readonly struct ParallelValueTaskAwaiter<T> : ICriticalNotifyCompletion, IParallelNotifyCompletion
{
    private readonly ParallelValueTask<T> _valueTask;

    internal ParallelValueTaskAwaiter(in ParallelValueTask<T> valueTask)
    {
        _valueTask = valueTask;
    }

    public bool IsCompleted
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            var implementation = _valueTask.Implementation;
            if (implementation is null)
            {
                // return true in this case, sync return with result
                return true;
            }

            return implementation.IsCompleted;
        }
    }

    public bool RequireContinuationToBeSetBeforeResult
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            var implementation = _valueTask.Implementation;
            if (implementation == null)
            {
                // return false in this case. If we return synchronously, then there is no forking inside 
                return false;
            }

            return implementation.RequireContinuationToBeSetBeforeResult;
        }
    }

    public void ParallelOnCompleted<TStateMachine>(TStateMachine stateMachine)
        where TStateMachine : IAsyncStateMachine
    {
        var implementation = _valueTask.Implementation;
        if (implementation == null)
        {
            stateMachine.MoveNext();
            return;
        }

        implementation.ParallelOnCompleted(stateMachine);
    }

    public void OnCompleted(Action continuation)
    {
        var implementation = _valueTask.Implementation;
        if (implementation == null)
        {
            continuation.Invoke();
            return;
        }

        implementation.OnCompleted(continuation);
    }

    public void UnsafeOnCompleted(Action continuation)
    {
        var implementation = _valueTask.Implementation;
        if (implementation == null)
        {
            continuation.Invoke();
            return;
        }

        implementation.UnsafeOnCompleted(continuation);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetResult()
    {
        var implementation = _valueTask.Implementation;
        if (implementation == null)
        {
            return _valueTask.Result!;
        }

        var taskResult = implementation.GetResult();
        if (!taskResult.HasResult)
        {
            taskResult.ExceptionDispatchInfo.Throw();
        }

        return taskResult.Result;
    }
}