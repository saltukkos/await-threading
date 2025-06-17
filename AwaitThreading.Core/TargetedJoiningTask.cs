// MIT License
// Copyright (c) 2024 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;
using AwaitThreading.Core.Context;

namespace AwaitThreading.Core;

public readonly struct TargetedJoiningTask
{
    public struct TargetedJoiningTaskAwaiter : ICriticalNotifyCompletion, IParallelNotifyCompletion
    {
        public bool IsCompleted => false;

        public void ParallelOnCompleted<TStateMachine>(TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine
        {
            var context = ParallelContextStorage.PopFrame();

            if (context.Id == 0)
            {
                context.JoinBarrier.SignalAndWait();
                stateMachine.MoveNext();
            }
            else
            {
                context.JoinBarrier.Signal();
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

    public TargetedJoiningTaskAwaiter GetAwaiter() => new();
}