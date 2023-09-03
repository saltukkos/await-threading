//MIT License
//Copyright (c) 2023 Saltuk Konstantin
//See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;

namespace AwaitThreading.Core;

public sealed class ForkingTask
{
    public readonly struct ForkingAwaiter : ICriticalNotifyCompletion, IParallelContextHandler
    {
        private readonly int _threadsCount;

        public ForkingAwaiter(int threadsCount)
        {
            _threadsCount = threadsCount;
        }

        public bool IsCompleted => false;
    
        public void OnCompleted(Action continuation)
        {
            var threadsCount = _threadsCount;
            var currentContext = ParallelContext.CaptureParallelContext();
            for (var i = 0; i < threadsCount; ++i)
            {
                var id = i;
                Task.Run(() =>
                {
                    ParallelContext.SetParallelContext(currentContext);
                    ParallelContext.PushFrame(new (id, threadsCount));
                    continuation.Invoke();
                }); //TODO exception handling
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