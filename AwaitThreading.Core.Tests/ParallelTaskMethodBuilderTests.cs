// MIT License
// Copyright (c) 2024 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
namespace AwaitThreading.Core.Tests;

[TestFixture]
[TestOf(typeof(ParallelTaskMethodBuilder))]
[TestOf(typeof(ParallelTaskMethodBuilder<>))]
public class ParallelTaskMethodBuilderTests
{
    [Test]
    public async Task Await_VoidResultSetSync_ResultReturned()
    {
        await TestBody().WaitAsync();
        return;

        async ParallelTask TestBody()
        {
            await GetResult();
        }

        async ParallelTask GetResult()
        {
        }
    }

    [Test]
    public async Task Await_IntResultSetSync_ResultReturned()
    {
        var result = await TestBody().WaitAsync();
        Assert.That(result, Is.EqualTo(42));
        return;

        async ParallelTask<int> TestBody()
        {
            return await GetResult();
        }

        async ParallelTask<int> GetResult()
        {
            return 42;
        }
    }

    [Test]
    public async Task Await_ResultSetAfterNonParallelTaskAwait_ResultReturned()
    {
        var result = await TestBody().WaitAsync();
        Assert.That(result, Is.EqualTo(42));
        return;

        async ParallelTask<int> TestBody()
        {
            return await GetResult();
        }

        async ParallelTask<int> GetResult()
        {
            await Task.Yield();
            return 42;
        }
    }

    [Test]
    public async Task Await_ResultSetAfterParallelOperations_ResultReturned()
    {
        var result = await TestBody().WaitAsync();
        Assert.That(result, Is.EqualTo(42));
        return;

        async ParallelTask<int> TestBody()
        {
            return await GetResult();
        }

        async ParallelTask<int> GetResult()
        {
            await new ForkingTask(2);
            await new JoiningTask();
            return 42;
        }
    }

    [Test]
    public async Task Await_ResultSetInTwoThreads_BothReturned()
    {
        var result = await TestBody().WaitAsync();
        Assert.That(result, Is.EqualTo(3));
        return;

        async ParallelTask<int> TestBody()
        {
            var sum = new ParallelCounter();
            var res = await ForkAndGetResult();
            sum.Add(res);
            await new JoiningTask();
            return sum.Count;
        }

        async ParallelTask<int> ForkAndGetResult()
        {
            await new ForkingTask(2);
            return ParallelContext.GetCurrentFrame().Id == 0 ? 1 : 2;
        }
    }

    [Test]
    public async Task AwaitVoid_ExceptionIsThrownInSyncContext_ExceptionIsPropagated()
    {
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => TestBody().WaitAsync());
        return;

        async ParallelTask TestBody()
        {
            throw new ArgumentOutOfRangeException();
        }
    }

    [Test]
    public async Task AwaitWithResult_ExceptionIsThrownInSyncContext_ExceptionIsPropagated()
    {
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => TestBody().WaitAsync());
        return;

        async ParallelTask<int> TestBody()
        {
            throw new ArgumentOutOfRangeException();
        }
    }

    [Test]
    public async Task AwaitVoid_ExceptionIsThrownInSubMethodSync_ExceptionIsPropagated()
    {
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => TestBody().WaitAsync());
        return;

        async ParallelTask TestBody()
        {
            await InnerMethod();
        }
        
        async ParallelTask InnerMethod()
        {
            throw new ArgumentOutOfRangeException();
        }
    }

    [Test]
    public async Task AwaitWithResult_ExceptionIsThrownInSubMethodSync_ExceptionIsPropagated()
    {
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => TestBody().WaitAsync());
        return;

        async ParallelTask<int> TestBody()
        {
            return await InnerMethod();
        }
        
        async ParallelTask<int> InnerMethod()
        {
            throw new ArgumentOutOfRangeException();
        }
    }

    [Test]
    public async Task Await_ExceptionIsThrownInAsyncContextDepth_ExceptionIsPropagated()
    {
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => TestBody().WaitAsync());
        return;

        async ParallelTask<int> TestBody()
        {
            await new ForkingTask(2);
            await new JoiningTask();
            throw new ArgumentOutOfRangeException();
        }
    }
}