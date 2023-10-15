//MIT License
//Copyright (c) 2023 Saltuk Konstantin
//See the LICENSE file in the project root for more information.

using System.Diagnostics;
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
            var barrier = new MyBarrier(threadsCount);
            for (var i = 0; i < threadsCount; ++i)
            {
                var id = i;
                Console.Out.WriteLine($"Schedule running of {id}");
                var stopwatch = Stopwatch.StartNew();
                //Task.Run(() =>
                Task.Factory.StartNew(() =>
                {
                    // Console.Out.WriteLine($"Actually run of {id} (took {stopwatch.Elapsed.TotalMilliseconds})");

                    if (stopwatch.Elapsed.TotalMilliseconds > 100)
                    {
                        Console.Out.WriteLine($"$ACHTUNG!!!!!!!!!!!!!!!!!!!!!!! {stopwatch.Elapsed.TotalMilliseconds} to start a task {id}");
                    }
                    
                    if (currentContext is not null)
                    {
                        ExecutionContext.Restore(currentContext);
                    }

                    ParallelContext.PushFrame(new (id, threadsCount, barrier));
                    continuation.Invoke();
                // }); //TODO exception handling
                }); 
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

    private readonly ForkingAwaiter _awaiter;

    public ForkingTask(int threadsCount)
    {
        _awaiter = new ForkingAwaiter(threadsCount);
    }

    public ForkingAwaiter GetAwaiter() => _awaiter;
}