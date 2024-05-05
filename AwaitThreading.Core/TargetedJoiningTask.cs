// MIT License
// Copyright (c) 2024 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;

namespace AwaitThreading.Core;

public readonly struct TargetedJoiningTask
{
    public struct TargetedJoiningTaskAwaiter : ICriticalNotifyCompletion, IParallelNotifyCompletion
    {
        public bool IsCompleted => false;
        public bool RequireContinuationToBeSetBeforeResult => false;

        public void ParallelOnCompleted(Action continuation)
        {
            var context = ParallelContext.PopFrame();

            if (context.Id == 0)
            {
                context.JoinBarrier.SignalAndWait();
                continuation.Invoke();
            }
            else
            {
                context.JoinBarrier.Signal();
            }
        }

        public void OnCompleted(Action continuation)
        {
            Assertion.ThrowBadAwait();
        }

        public void UnsafeOnCompleted(Action continuation)
        {
            Assertion.ThrowBadAwait();
        }

        public void GetResult()
        {
        }
    }

    public TargetedJoiningTaskAwaiter GetAwaiter() => new();
}