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

        public void ParallelOnCompleted<TStateMachine>(TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine
        {
            var forkingClosure = new ForkingClosure<TStateMachine>(stateMachine, _threadsCount);

            for (var i = 0; i < _threadsCount; ++i)
            {
                Logger.Log("Scheduling task " + i);
                Task.Factory.StartNew(
                    static args =>
                    {
                        try
                        {
                            ((ForkingClosure<TStateMachine>)args!).StartNewThread();
                        }
                        finally
                        {
                            ParallelContext.CaptureAndClear();
                        }
                    },
                    forkingClosure,
                    TaskCreationOptions.DenyChildAttach); //TODO: DenyChildAttach?
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

public class ForkingClosure<TStateMachine>
    where TStateMachine : IAsyncStateMachine
{
    private readonly TStateMachine _stateMachine;
    private readonly ExecutionContext? _executionContext;
    private readonly SingleWaiterBarrier _barrier;
    private readonly ParallelContext _parallelContext;
    private int _myThreadId = -1;

    public ForkingClosure(TStateMachine stateMachine, int threadsCount)
    {
        _executionContext = ExecutionContext.Capture();
        _stateMachine = stateMachine.MakeCopy();
        _parallelContext = ParallelContext.Capture();
        _barrier = new SingleWaiterBarrier(threadsCount);
    }

    public void StartNewThread()
    {
        if (_executionContext is not null)
        {
            ExecutionContext.Restore(_executionContext);
        }

        ParallelContext.Restore(_parallelContext); //TODO: can avoid second set
        ParallelContext.PushFrame(new ParallelFrame(Interlocked.Increment(ref _myThreadId), _barrier.Count, _barrier));
        Logger.Log("Task started");
        _stateMachine.MakeCopy().MoveNext();
    }
}