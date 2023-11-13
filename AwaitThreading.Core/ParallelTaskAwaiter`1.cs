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

    public bool RequireContinuationToBeSetBeforeResult => _task.RequireContinuationToBeSetBeforeResult;

    public void ParallelOnCompleted(Action continuation)
    {
        _task.SetContinuation(continuation);
    }
    
    public void OnCompleted(Action continuation)
    {
        Assertion.ThrowInvalidTaskIsUsed();
    }

    public void UnsafeOnCompleted(Action continuation)
    {
        Assertion.ThrowInvalidTaskIsUsed();
    }

    public T GetResult() => _task.GetResult();
    // public T GetResult([CallerMemberName] string? c = null, [CallerLineNumber] int callerLine = 0) => _task.GetResult($"{c}:{callerLine}");

}