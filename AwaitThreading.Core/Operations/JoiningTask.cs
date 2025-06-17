//MIT License
//Copyright (c) 2023 Saltuk Konstantin
//See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;
using AwaitThreading.Core.Context;
using AwaitThreading.Core.Diagnostics;
using AwaitThreading.Core.Tasks;

namespace AwaitThreading.Core.Operations;

public readonly struct JoiningTask
{
    public readonly struct JoiningTaskAwaiter : ICriticalNotifyCompletion, IParallelNotifyCompletion
    {
        public bool IsCompleted => false;

        public void ParallelOnCompleted<TStateMachine>(TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine
        {
            Logger.Log("Start joining");
            var context = ParallelContextStorage.PopFrame();

            if (context.JoinBarrier.Finish())
            {
                stateMachine.MoveNext();
            }
            else
            {
                ParallelContextStorage.CaptureAndClear();
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