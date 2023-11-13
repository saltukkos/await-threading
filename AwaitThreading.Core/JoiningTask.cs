//MIT License
//Copyright (c) 2023 Saltuk Konstantin
//See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;

namespace AwaitThreading.Core;

public sealed class JoiningTask
{
    public readonly struct JoiningTaskAwaiter : ICriticalNotifyCompletion, IParallelNotifyCompletion
    {
        public JoiningTaskAwaiter()
        {
        }

        public bool IsCompleted => false;

        public void ParallelOnCompleted(Action continuation)
        {
            var context = ParallelContext.PopFrame();
            // context.JoinBarrier.SignalAndWait(); //TODO do not block threads with id != 0

            if (context.Id == 0)
            {
                context.JoinBarrier.SignalAndWait();
//                context.JoinBarrier.Dispose();
                continuation.Invoke();
            }
            else
            {
                context.JoinBarrier.Signal();
            }
        }

        public void OnCompleted(Action continuation)
        {
            Assertion.ThrowInvalidTaskIsUsed();
        }

        public void UnsafeOnCompleted(Action continuation)
        {
            Assertion.ThrowInvalidTaskIsUsed();
        }

        public void GetResult()
        {
        }
    }

    private readonly JoiningTaskAwaiter _awaiter = new();
    public JoiningTaskAwaiter GetAwaiter() => _awaiter;
}