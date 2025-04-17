//MIT License
//Copyright (c) 2023 Saltuk Konstantin
//See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;

namespace AwaitThreading.Core;

public readonly struct JoiningTask
{
    public struct JoiningTaskAwaiter : ICriticalNotifyCompletion, IParallelNotifyCompletion
    {
        public bool IsCompleted => false;

        public void ParallelOnCompleted<TStateMachine>(TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine
        {
            var context = ParallelContext.PopFrame();

            if (context.JoinBarrier.Finish())
            {
                stateMachine.MoveNext();
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

    public JoiningTaskAwaiter GetAwaiter() => new();
}