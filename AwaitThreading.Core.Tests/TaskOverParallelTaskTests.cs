// MIT License
// Copyright (c) 2024 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
namespace AwaitThreading.Core.Tests;

[TestFixture]
public class TaskOverParallelTaskTests : BaseClassWithParallelContextValidation
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
    public async Task Await_RegularTaskHasDirectJoin_InvalidOperationExceptionIsThrows()
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
    public async Task Await_RegularTaskAwaitsJoiningMethod_InvalidOperationExceptionIsThrows()
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

    [Test]
    public async Task Await_ParallelTaskHasUnpairedJoin_InvalidOperationExceptionIsThrows_2()
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
            await new JoiningTask();
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

    [Test, Description(@"In this test, InnerParallelTaskBody starts Joining in sync phase,
there is no way to differentiate this case from regular Join inside `ParallelTaskBody`,
so the first join passes and the second join throws since ParallelContext is empty")]
    public async Task Await_ParallelTaskHasUnpairedJoin2_InvalidOperationExceptionIsThrows()
    {
        var exceptionsCounter = new ParallelCounter();
        using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        await TestBody().WaitAsync(tokenSource.Token);
        return;

        async Task TestBody()
        {
            await ParallelTaskBody();
            exceptionsCounter.AssertCount(1);
        }

        async ParallelTask ParallelTaskBody()
        {
            await new ForkingTask(2);
            await StandardTaskMethod();
            try
            {
                await new JoiningTask();
            }
            catch (InvalidOperationException)
            {
                exceptionsCounter.Increment();
            }
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

    [Test, Description(@"In this test, `InnerParallelTaskBody` starts Joining in async phase,
so ParallelMethodBuilder already detected this and cleared the ParallelContext, this way
Join throws and context is not changed.")]
    public async Task Await_ParallelTaskHasUnpairedJoin3_InvalidOperationExceptionIsThrows()
    {
        var exceptionsCounter = new ParallelCounter();
        using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        await TestBody().WaitAsync(tokenSource.Token);
        return;

        async Task TestBody()
        {
            await ParallelTaskBody();
            Assert.That(exceptionsCounter.Count, Is.EqualTo(2));
        }

        async ParallelTask ParallelTaskBody()
        {
            await new ForkingTask(2);
            await StandardTaskMethod();
            await new JoiningTask();
        }

        async Task StandardTaskMethod()
        {
            await Task.Yield();
            await InnerParallelTaskBody();
        }

        async ParallelTask InnerParallelTaskBody()
        {
            try
            {
                await new JoiningTask();
            }
            catch (InvalidOperationException)
            {
                exceptionsCounter.Increment();
            }
        }
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public async Task Await_ParallelTaskHasUnpairedJoin4_InvalidOperationExceptionIsThrows(bool hasYieldBeforeInnerJoin)
    {
        var exceptionsCounter = new ParallelCounter();
        using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        await TestBody().WaitAsync(tokenSource.Token);
        return;

        async Task TestBody()
        {
            await ParallelTaskBody();
            Assert.That(exceptionsCounter.Count, Is.EqualTo(1));
        }

        async ParallelTask ParallelTaskBody()
        {
            await new ForkingTask(2);
            await StandardTaskMethod();

            try
            {
                await new JoiningTask();
            }
            catch (InvalidOperationException)
            {
                exceptionsCounter.Increment();
            }
        }

        async Task StandardTaskMethod()
        {
            await InnerParallelTaskBody();
        }

        async ParallelTask InnerParallelTaskBody()
        {
            if (hasYieldBeforeInnerJoin)
            {
                await Task.Yield(); // it does not matter, but I would expect this to go non-sync and therefore clear the context
            }

            await new JoiningTask();
        }
    }

    
    [Test]
    public async Task Await_ParallelTaskHasUnpairedJoin41_InvalidOperationExceptionIsThrows()
    {
        var exceptionsCounter = new ParallelCounter();
        using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        await TestBody().WaitAsync(tokenSource.Token);
        return;

        async Task TestBody()
        {
            await ParallelTaskBody();
            Assert.That(exceptionsCounter.Count, Is.EqualTo(2));
        }

        async ParallelTask ParallelTaskBody()
        {
            await new ForkingTask(2);
            await StandardTaskMethod();
            await new JoiningTask();
        }

        async Task StandardTaskMethod()
        {
            await InnerParallelTaskBody();
        }

        async ParallelTask InnerParallelTaskBody()
        {
            await Task.Yield();
            await new JoiningTask();
        }
    }

    [Test]
    public async Task Await_ContextIsEmptyOnReturnFromAsync()
    {
        var exceptionsCounter = new ParallelCounter();
        using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        await TestBody().WaitAsync(tokenSource.Token);
        return;

        async Task TestBody()
        {
            await ParallelTaskBody();
            Assert.That(exceptionsCounter.Count, Is.EqualTo(2));
        }

        async ParallelTask ParallelTaskBody()
        {
            await new ForkingTask(1);
            await new ForkingTask(2);
            var task = StandardTaskMethod();
            Assert.That(ParallelContext.GetCurrentFrameSafe(), Is.Null);

            try
            {
                await new JoiningTask();
            }
            catch (InvalidOperationException)
            {
                exceptionsCounter.Increment();
            }

            await task;
            await new JoiningTask();
        }

        async Task StandardTaskMethod()
        {
            var task = InnerParallelTaskBody();
            Assert.That(ParallelContext.GetCurrentFrameSafe(), Is.Null);
            await task;
        }

        async ParallelTask InnerParallelTaskBody()
        {
            await Task.Yield();
            await new JoiningTask();
        }
    }

    [Test]
    public async Task Await_ParallelTaskHasUnpairedJoin5_InvalidOperationExceptionIsThrows()
    {
        var exceptionsCounter = new ParallelCounter();
        using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(1000));
        await TestBody().WaitAsync(tokenSource.Token);
        return;

        async Task TestBody()
        {
            await ParallelTaskBody();
            //Assert.That(exceptionsCounter.Count, Is.EqualTo(2));
        }

        async ParallelTask ParallelTaskBody()
        {
            await Task.Yield();
            await new ForkingTask(1);
            await StandardTaskMethod();
            await new JoiningTask();
        }

        async Task StandardTaskMethod()
        {
            await InnerParallelTaskBody();
        }

        async ParallelTask InnerParallelTaskBody()
        {
            await new ForkingTask(1);
            await new JoiningTask();
            await new JoiningTask();
        }
    }
}