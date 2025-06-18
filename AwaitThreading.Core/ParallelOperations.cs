// MIT License
// Copyright (c) 2025 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;
using AwaitThreading.Core.Context;
using AwaitThreading.Core.Operations;
using AwaitThreading.Core.Tasks;

namespace AwaitThreading.Core;

public static class ParallelOperations
{
    /// <summary>
    /// Returns a task that performs Fork operation while awaiting.
    /// After the Fork operation, <paramref name="threadCount"/> threads will execute
    /// the method starting from the next line. Forks support nesting, e.g. each forked thread
    /// can also perform Fork operations.
    /// </summary>
    /// <param name="threadCount">Number of threads for Fork operation</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="threadCount"/> is zero or negative</exception>
    /// <exception cref="InvalidOperationException">Return type of the calling method is not <see cref="ParallelTask"/>, <see cref="ParallelTask{T}"/> or <see cref="ParallelValueTask"/>-alternative. This exception is thrown during the `await` operation.</exception>
    /// <returns>The id of current thread, from 0 to (<paramref name="threadCount"/> - 1) inclusive</returns>
    /// <example>
    /// <code>
    /// async ParallelTask MyAsyncMethod()
    /// {
    ///     var id = await ParallelOperations.Fork(2);
    ///     Console.Out.WriteLine($"Hello World from thread {id}");
    ///     await Task.Delay(100); // any async workload
    ///     Thread.Sleep(100); //any sync workload
    ///     await ParallelOperations.Join();
    /// }
    /// </code>
    /// </example>
    public static ForkingTaskWithId Fork(int threadCount)
    {
        return new ForkingTaskWithId(new ForkingTask(threadCount));
    }

    /// <summary>
    /// Returns a task that performs Join operation while awaiting.
    /// One of the previously forked threads will proceed after the Join operation.
    /// </summary>
    /// <exception cref="InvalidOperationException">Threads are not currently forked, e.g. <see cref="ParallelContextStorage.CurrentThreadContext"/> is empty. This exception is thrown during the `await` operation.</exception>
    public static JoiningTask Join()
    {
        return new JoiningTask();
    }

    /// <summary>
    /// Returns a task that performs Join operation while awaiting.
    /// Thread with ID=0 will proceed after the Join operation.
    /// </summary>
    /// <exception cref="InvalidOperationException">Threads are not currently forked, e.g. <see cref="ParallelContextStorage.CurrentThreadContext"/> is empty. This exception is thrown during the `await` operation.</exception>
    public static TargetedJoiningTask JoinOnMainThread()
    {
        return new TargetedJoiningTask();
    }
}

public readonly struct ForkingTaskWithId
{
    private readonly ForkingTask _forkingTask;

    public ForkingTaskWithId(ForkingTask forkingTask)
    {
        _forkingTask = forkingTask;
    }

    public ForkingAwaiterWithId GetAwaiter() => new(_forkingTask.GetAwaiter());

    public readonly struct ForkingAwaiterWithId : ICriticalNotifyCompletion, IParallelNotifyCompletion
    {
        private readonly ForkingTask.ForkingAwaiter _forkingAwaiter;
        
        public ForkingAwaiterWithId(ForkingTask.ForkingAwaiter forkingAwaiter)
        {
            _forkingAwaiter = forkingAwaiter;
        }

        public bool IsCompleted => _forkingAwaiter.IsCompleted;

        public void ParallelOnCompleted<TStateMachine>(TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine
        {
            _forkingAwaiter.ParallelOnCompleted(stateMachine);
        }

        public void OnCompleted(Action continuation)
        {
            _forkingAwaiter.OnCompleted(continuation);
        }

        public void UnsafeOnCompleted(Action continuation)
        {
            _forkingAwaiter.UnsafeOnCompleted(continuation);
        }

        public int GetResult()
        {
            return ParallelContextStorage.GetTopFrameId();
        }
    }

}