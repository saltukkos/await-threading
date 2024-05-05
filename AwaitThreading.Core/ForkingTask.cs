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

        private class ActionClosure
        {
            public ExecutionContext? ExecutionContext;
            public ParallelFrame ParallelFrame;
            public Action Continuation;
        }
        
        public void ParallelOnCompleted(Action continuation)
        {
            var threadsCount = _threadsCount;
            var currentContext = ExecutionContext.Capture();
            var barrier = new SingleWaiterBarrier(threadsCount);

            for (var i = 0; i < threadsCount; ++i)
            {
                var actionClosure = new ActionClosure
                {
                    ExecutionContext = currentContext,
                    ParallelFrame = new ParallelFrame(i, threadsCount, barrier),
                    Continuation = continuation,
                };
                
                Logger.Log("Scheduling task " + i);
                Task.Factory.StartNew(
                    static args =>
                    {
                        Logger.Log("Task started");

                        var parameters = (ActionClosure)args!;
                        var executionContext = parameters.ExecutionContext;
                        if (executionContext is not null)
                        {
                            ExecutionContext.Restore(executionContext);
                        }

                        ParallelContext.PushFrame(parameters.ParallelFrame);
                        parameters.Continuation.Invoke();
                    },
                    actionClosure,
                    CancellationToken.None,
                    TaskCreationOptions.DenyChildAttach | TaskCreationOptions.RunContinuationsAsynchronously,
                    TaskScheduler.Default);
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