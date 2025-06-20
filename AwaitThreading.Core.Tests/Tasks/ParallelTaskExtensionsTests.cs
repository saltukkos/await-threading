// MIT License
// Copyright (c) 2025 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

using AwaitThreading.Core.Tasks;
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace AwaitThreading.Core.Tests.Tasks;

[TestFixture]
[TestOf(typeof(ParallelTaskExtensions))]
public class ParallelTaskExtensionsTests
{
    [Test]
    public async Task AsTask_GetSyncResult_ReturnsResult()
    {
        var result = await ParallelMethod().AsTask();
        Assert.That(result, Is.EqualTo(42));

        async ParallelTask<int> ParallelMethod()
        {
            return 42;
        }
    }

    [Test]
    public async Task AsTask_GetAsyncResult_ReturnsResult()
    {
        var result = await ParallelMethod().AsTask();
        Assert.That(result, Is.EqualTo(42));

        async ParallelTask<int> ParallelMethod()
        {
            await Task.Yield();
            return 42;
        }
    }

    [Test]
    public async Task AsTask_GetAsyncResultAfterParallelOperations_ReturnsResult()
    {
        var result = await ParallelMethod().AsTask();
        Assert.That(result, Is.EqualTo(42));

        async ParallelTask<int> ParallelMethod()
        {
            await ParallelOperations.Fork(2);
            await ParallelOperations.Join();
            return 42;
        }
    }

    [Test]
    public async Task AsTask_GetAsyncResult_ExceptionRethrown()
    {
        await AssertEx.CheckThrowsAsync<ArgumentOutOfRangeException>(async () => await ParallelMethod().AsTask());

        async ParallelTask<int> ParallelMethod()
        {
            await Task.Yield();
            throw new ArgumentOutOfRangeException();
        }
    }

    [Test]
    public async Task AsTask_GetAsyncResultAfterParallelOperations_ExceptionRethrown()
    {
        await AssertEx.CheckThrowsAsync<ArgumentOutOfRangeException>(async () => await ParallelMethod().AsTask());

        async ParallelTask<int> ParallelMethod()
        {
            await ParallelOperations.Fork(2);
            await ParallelOperations.Join();
            throw new ArgumentOutOfRangeException();
        }
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task AsTask_WaitSynchronouslyForAsyncResult_ReturnsResult(bool useParallel)
    {
        Assert.That(ParallelMethod().AsTask().Result, Is.EqualTo(42));

        async ParallelTask<int> ParallelMethod()
        {
            if (useParallel)
            {
                await ParallelOperations.Fork(2);
                await ParallelOperations.Join();
            }
            else
            {
                await Task.Yield();
            }

            return 42;
        }
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task AsValueTask_WaitSynchronouslyForAsyncResult_ReturnsResult(bool useParallel)
    {
        Assert.That(ParallelMethod().AsValueTask().Result, Is.EqualTo(42));

        async ParallelValueTask<int> ParallelMethod()
        {
            if (useParallel)
            {
                await ParallelOperations.Fork(2);
                await ParallelOperations.Join();
            }
            else
            {
                await Task.Yield();
            }

            return 42;
        }
    }

    [Test]
    public async Task AsValueTask_GetSyncResult_ReturnsResultSynchronously()
    {
        var valueTask = new ParallelValueTask<int>(42).AsValueTask();
        Assert.That(valueTask.IsCompleted, Is.True);
        Assert.That(valueTask.Result, Is.EqualTo(42));
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task AsValueTask_GetAsyncSyncResult_ReturnsResultAsynchronously(bool actuallyAsync)
    {
        var valueTask = ParallelMethod().AsValueTask();
        Assert.That(valueTask.IsCompleted, Is.False); // Always false, even when actuallyAsync is false.
                                                      // Tasks are "cold" and are not started until await  
        Assert.That(await valueTask, Is.EqualTo(42));

        async ParallelValueTask<int> ParallelMethod()
        {
            if (actuallyAsync)
            {
                await Task.Yield();
            }

            return 42;
        }
    }

    
}