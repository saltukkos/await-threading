//MIT License
//Copyright (c) 2023 Saltuk Konstantin
//See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;

namespace AwaitThreading.Core;

public readonly struct ParallelTaskAwaiter<T> : ICriticalNotifyCompletion, IParallelNotifyCompletion
{
    private readonly ParallelTask<T> _task;

    public ParallelTaskAwaiter(in ParallelTask<T> task)
    {
        _task = task;
    }

    public bool IsCompleted => false;
    
    public void ParallelOnCompleted(Action continuation)
    {
        _task.SetContinuation(continuation);
    }
    
    public void OnCompleted(Action continuation)
    {
        throw new NotSupportedException("Only ParallelTask methods are supported");
    }

    public void UnsafeOnCompleted(Action continuation)
    {
        throw new NotSupportedException("Only ParallelTask methods are supported");
    }

    public T GetResult() => _task.GetResult();
}