//MIT License
//Copyright (c) 2023 Saltuk Konstantin
//See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;

namespace AwaitThreading.Core;

public sealed class ForkingTask
{
    public readonly struct ForkingAwaiter : ICriticalNotifyCompletion, IParallelNotifyCompletion
    {
        private readonly int _threadsCount;

        public ForkingAwaiter(int threadsCount)
        {
            _threadsCount = threadsCount;
        }

        public bool IsCompleted => false;

        public bool RequireContinuationToBeSetBeforeResult => true;

        public void ParallelOnCompleted(Action continuation)
        {
            var threadsCount = _threadsCount;
            var currentContext = ExecutionContext.Capture();
            var barrier = new SingleWaiterBarrier(threadsCount);
            for (var i = 0; i < threadsCount; ++i)
            {
                var id = i;
                Task.Run(() =>
                {
                    if (currentContext is not null)
                    {
                        ExecutionContext.Restore(currentContext);
                    }

                    ParallelContext.PushFrame(new ParallelFrame(id, threadsCount, barrier));
                    continuation.Invoke();
                }); //TODO exception handling
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

    private readonly ForkingAwaiter _awaiter;

    public ForkingTask(int threadsCount)
    {
        if (threadsCount <= 0)
            Assertion.ThrowInvalidTasksCount();

        _awaiter = new ForkingAwaiter(threadsCount);
    }

    public ForkingAwaiter GetAwaiter() => _awaiter;
}