// MIT License
// Copyright (c) 2024 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
namespace AwaitThreading.Core.Tests;

[TestFixture]
public class TaskOverParallelTaskTests
{
    [Test]
    public async Task Await_ParallelTaskIsSimple_Success()
    {
        var result = await TestBody();
        Assert.That(result, Is.EqualTo(42));
        return;

        async ParallelTask<int> TestBody()
        {
            return 42;
        }
    }

    [Test]
    public async Task Await_ParallelTaskHasPairedForkAndJoin_Success()
    {
        var result = await TestBody();
        Assert.That(result, Is.EqualTo(42));
        return;

        async ParallelTask<int> TestBody()
        {
            await new ForkingTask(2);
            await new JoiningTask();
            return 42;
        }
    }

    [Test]
    public async Task Await_ParallelTaskThrowsException_ExceptionIsPropagated()
    {
        await AssertEx.CheckThrowsAsync<AssertionException>(TestBody);
        return;

        async Task TestBody()
        {
            await TestBodyInner();
        }

        async ParallelTask TestBodyInner()
        {
            throw new AssertionException("Message");
        }
    }

    [Test]
    public async Task Await_ParallelTaskHasUnpairedFork_InvalidOperationExceptionIsThrows()
    {
        await AssertEx.CheckThrowsAsync<InvalidOperationException>(TestBody);
        return;

        async Task TestBody()
        {
            await TestBodyInner();
        }

        async ParallelTask TestBodyInner()
        {
            await new ForkingTask(2);
        }
    }

    [Test, Ignore("AsyncTaskMethodBuilder propagate exception to thread pool, can't handle it in test")]
    public async Task Await_DirectFork_InvalidOperationExceptionIsThrows()
    {
        await AssertEx.CheckThrowsAsync<InvalidOperationException>(TestBody);
        return;

        async Task TestBody()
        {
            await new ForkingTask(2);
        }
    }

    [Test, Ignore("AsyncTaskMethodBuilder propagate exception to thread pool, can't handle it in test")]
    public async Task Await_DirectJoin_InvalidOperationExceptionIsThrows()
    {
        await AssertEx.CheckThrowsAsync<InvalidOperationException>(TestBody);
        return;

        async Task TestBody()
        {
            await new JoiningTask();
        }
    }

    [Test, Ignore("AsyncTaskMethodBuilder propagate exception to thread pool, can't handle it in test")]
    public async Task Await_ParallelTaskHasDirectJoin_InvalidOperationExceptionIsThrows()
    {
        await AssertEx.CheckThrowsAsync<InvalidOperationException>(TestBody);
        return;

        async Task TestBody()
        {
            await ParallelTaskBody();
        }

        async ParallelTask ParallelTaskBody()
        {
            await new ForkingTask(2);
            await StandardTaskMethod();
        }

        async Task StandardTaskMethod()
        {
            await new JoiningTask();
        }
    }

    [Test]
    public async Task Await_ParallelTaskHasUnpairedJoin_InvalidOperationExceptionIsThrows()
    {
        await AssertEx.CheckThrowsAsync<InvalidOperationException>(TestBody);
        return;

        async Task TestBody()
        {
            await ParallelTaskBody();
        }

        async ParallelTask ParallelTaskBody()
        {
            await new ForkingTask(2);
            await StandardTaskMethod();
        }

        async Task StandardTaskMethod()
        {
            await InnerParallelTaskBody();
        }

        async ParallelTask InnerParallelTaskBody()
        {
            await new JoiningTask();
        }
    }
}