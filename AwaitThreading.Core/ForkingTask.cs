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

        public void ParallelOnCompleted(Action continuation)
        {
            var threadsCount = _threadsCount;
            var currentContext = ExecutionContext.Capture();
            var barrier = new Barrier(threadsCount);
            for (var i = 0; i < threadsCount; ++i)
            {
                var id = i;
                Task.Run(() =>
                {
                    if (currentContext is not null)
                    {
                        ExecutionContext.Restore(currentContext);
                    }

                    ParallelContext.PushFrame(new (id, threadsCount, barrier));
                    continuation.Invoke();
                }); //TODO exception handling
            }
        }

        public void OnCompleted(Action continuation)
        {
            throw new NotSupportedException("Only ParallelTask methods are supported");
        }

        public void UnsafeOnCompleted(Action continuation)
        {
            OnCompleted(continuation);
        }

        public void GetResult()
        {
        }
    }

    private readonly ForkingAwaiter _awaiter;

    public ForkingTask(int threadsCount)
    {
        _awaiter = new ForkingAwaiter(threadsCount);
    }

    public ForkingAwaiter GetAwaiter() => _awaiter;
}