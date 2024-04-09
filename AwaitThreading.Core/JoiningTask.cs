//MIT License
//Copyright (c) 2023 Saltuk Konstantin
//See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;

namespace AwaitThreading.Core;

public sealed class JoiningTask
{
    public struct JoiningTaskAwaiter : ICriticalNotifyCompletion, IParallelNotifyCompletion
    {
        public JoiningTaskAwaiter()
        {
        }

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

    private readonly JoiningTaskAwaiter _awaiter = new();
    public JoiningTaskAwaiter GetAwaiter() => _awaiter;
}