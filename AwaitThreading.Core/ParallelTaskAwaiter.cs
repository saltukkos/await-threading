// MIT License
// Copyright (c) 2023 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;

namespace AwaitThreading.Core;

public readonly struct ParallelTaskAwaiter : ICriticalNotifyCompletion, IParallelNotifyCompletion
{
    private readonly ParallelTaskImpl<Unit> _taskImpl;

    internal ParallelTaskAwaiter(ParallelTaskImpl<Unit> taskImpl)
    {
        _taskImpl = taskImpl;
    }

    public bool IsCompleted => _taskImpl.IsCompleted;

    public bool RequireContinuationToBeSetBeforeResult => _taskImpl.RequireContinuationToBeSetBeforeResult;

    public void ParallelOnCompleted(Action continuation) => _taskImpl.SetContinuation(continuation);

    public void OnCompleted(Action continuation) => Assertion.ThrowInvalidTaskIsUsed();

    public void UnsafeOnCompleted(Action continuation) => Assertion.ThrowInvalidTaskIsUsed();

    public void GetResult()
    {
        var taskResult = _taskImpl.GetResult();
        if (!taskResult.HasResult)
        {
            taskResult.ExceptionDispatchInfo.Throw();
        }
    }
}