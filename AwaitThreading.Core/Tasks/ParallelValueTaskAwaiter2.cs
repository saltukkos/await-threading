// MIT License
// Copyright (c) 2025 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;

namespace AwaitThreading.Core.Tasks;

public readonly struct ParallelValueTaskAwaiter : ICriticalNotifyCompletion, IParallelNotifyCompletion
{
    private readonly ParallelValueTask _valueTask;

    internal ParallelValueTaskAwaiter(in ParallelValueTask valueTask)
    {
        _valueTask = valueTask;
    }

    public bool IsCompleted
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _valueTask.Implementation is null;
    }

    public void ParallelOnCompleted<TStateMachine>(TStateMachine stateMachine)
        where TStateMachine : IAsyncStateMachine
    {
        var implementation = _valueTask.Implementation;
        if (implementation == null)
        {
            // Note: this should not happen within compiler-generated code,
            // sync return valueTasks have IsCompleted = true, call MoveNext just to be on a safe side.
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
    public void GetResult()
    {
        var implementation = _valueTask.Implementation;
        if (implementation == null)
        {
            return;
        }

        var taskResult = implementation.GetResult();
        if (!taskResult.HasResult)
        {
            taskResult.ExceptionDispatchInfo.Throw();
        }
    }
}