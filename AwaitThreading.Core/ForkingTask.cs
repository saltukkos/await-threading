//MIT License
//Copyright (c) 2023 Saltuk Konstantin
//See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;

namespace AwaitThreading.Core;

public sealed class ForkingTask
{
    public readonly struct ForkingAwaiter : ICriticalNotifyCompletion
    {
        private readonly int _threadsCount;

        public ForkingAwaiter(int threadsCount)
        {
            _threadsCount = threadsCount;
        }

        public bool IsCompleted => false;
    
        public void OnCompleted(Action continuation)
        {
            for (var i = 0; i < _threadsCount; ++i)
            {
                var id = i;
                Task.Run(() =>
                {
                    ParallelContext.PushContext(new ParallelContext(id));
                    continuation.Invoke();
                });
            }
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