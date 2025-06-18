//MIT License
//Copyright (c) 2023 Saltuk Konstantin
//See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;
using AwaitThreading.Core.Context;
using AwaitThreading.Core.Diagnostics;
using AwaitThreading.Core.Tasks;

namespace AwaitThreading.Core.Operations;

public sealed class ForkingTask
{
    public readonly struct ForkingAwaiter : ICriticalNotifyCompletion, IParallelNotifyCompletion
    {
        private readonly int _threadCount;

        public ForkingAwaiter(int threadCount)
        {
            _threadCount = threadCount;
        }

        public bool IsCompleted => false;

        public void ParallelOnCompleted<TStateMachine>(TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine
        {
            var forkingClosure = new ForkingClosure<TStateMachine>(
                stateMachine,
                _threadCount,
                ParallelContextStorage.CaptureAndClear());

            for (var i = 0; i < _threadCount; ++i)
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
                            ParallelContextStorage.ClearButNotExpected();
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

    public ForkingTask(int threadCount)
    {
        if (threadCount <= 0)
            Assertion.ThrowInvalidTasksCount(threadCount);

        _awaiter = new ForkingAwaiter(threadCount);
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

    public ForkingClosure(TStateMachine stateMachine, int threadCount, ParallelContext parallelContext)
    {
        _executionContext = ExecutionContext.Capture();
        _stateMachine = stateMachine.MakeCopy(); //TODO: redundant?
        _parallelContext = parallelContext;
        _barrier = new SingleWaiterBarrier(threadCount);
    }

    public void StartNewThread()
    {
        if (_executionContext is not null)
        {
            ExecutionContext.Restore(_executionContext);
        }

        // TODO: is race possible? Let's say some threads didn't start yet but one thread is already at 'await Join'.
        //  Do we need to read _barrier.Count before starting any threads? (closure will become larger :( )
        var newFrame = new ParallelFrame(Interlocked.Increment(ref _myThreadId), _barrier.Count, _barrier);
        ParallelContextStorage.CurrentThreadContext = _parallelContext.PushFrame(newFrame);
        Logger.Log("Task started");
        _stateMachine.MakeCopy().MoveNext();
    }
}