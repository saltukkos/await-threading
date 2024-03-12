//MIT License
//Copyright (c) 2023 Saltuk Konstantin
//See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;

namespace AwaitThreading.Core;

public readonly struct ParallelTaskAwaiter<T> : ICriticalNotifyCompletion, IParallelNotifyCompletion
{
    private readonly ParallelTaskImpl<T> _taskImpl;

    internal ParallelTaskAwaiter(ParallelTaskImpl<T> taskImpl)
    {
        _taskImpl = taskImpl;
    }

    //TODO: Am I sure? Why not with result and !RequireContinuationToBeSetBeforeResult?
    public bool IsCompleted => false;

    public bool RequireContinuationToBeSetBeforeResult => _taskImpl.RequireContinuationToBeSetBeforeResult;

    public void ParallelOnCompleted(Action continuation) => _taskImpl.SetContinuation(continuation);

    public void OnCompleted(Action continuation) => Assertion.ThrowInvalidTaskIsUsed();

    public void UnsafeOnCompleted(Action continuation) => Assertion.ThrowInvalidTaskIsUsed();

    public T GetResult()
    {
        var taskResult = _taskImpl.GetResult();
        if (!taskResult.HasResult)
        {
            taskResult.ExceptionDispatchInfo.Throw();
        }

        return taskResult.Result;
    }
}