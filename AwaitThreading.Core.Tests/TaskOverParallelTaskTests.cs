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

    [TestCase(true)]
    [TestCase(false)]
    public async Task Await_ParallelTaskHasUnpairedFork_InvalidOperationExceptionIsThrows(bool inSyncPart)
    {
        await AssertEx.CheckThrowsAsync<InvalidOperationException>(TestBody);
        return;

        async Task TestBody()
        {
            if (!inSyncPart)
            {
                await Task.Yield();
            }

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

        async ParallelTask<int> ParallelTaskBody()
        {
            if (ParallelContext.GetCurrentFrameSafe() is not null)
            {
                FailFast();
            }

            await new ForkingTask(2);
            
            if (ParallelContext.GetCurrentFrameSafe() is null)
            {
                FailFast();
            }

            var standardTaskMethod = StandardTaskMethod();
            try
            {
                await standardTaskMethod;
            }
            finally
            {
                if (ParallelContext.GetCurrentFrameSafe() is null)
                {
                    FailFast();
                }
            }

            return 1;
        }

        async Task StandardTaskMethod()
        {
            // Note: just explicitly check current behavior. In general, we would like to have an empty context here,
            // but it's not possible, since we do not have a control over sync execution 
            if (ParallelContext.GetCurrentFrameSafe() is null)
            {
                FailFast();
            }

            try
            {
                var innerParallelTaskBody = InnerParallelTaskBody();
                await innerParallelTaskBody;
            }
            finally
            {
                // NOTE: here we need to have an EMPTY context. Otherwise, standard TaskMethodBuilder can return
                // the thread with ParallelContext to the thread pool, since continuation is likely to be executed
                // on another thread, and we have no more control after over it.  
                if (ParallelContext.GetCurrentFrameSafe() is not null)
                {
                    FailFast();
                }
            }
        }

        async ParallelTask InnerParallelTaskBody()
        {
            if (ParallelContext.GetCurrentFrameSafe() is not null)
            {
                FailFast();
            }

            await new JoiningTask();
        }
    }

    [TestCase(1)]
    [TestCase(2)]
    public async Task Await_RegularTaskIsAwaited_ContextIsClearedAndRestored1(int threadsCount)
    {
        await TestBody();

        async ParallelTask TestBody()
        {
            await new ForkingTask(threadsCount);
            var regularAsyncMethod = RegularAsyncMethod();
            await regularAsyncMethod;

            Assert.That(ParallelContext.GetCurrentContext().IsEmpty, Is.False);
            await new JoiningTask();
        }

        async Task RegularAsyncMethod()
        {
            // Note: just explicitly check current behavior. In general, we would like to have an empty context here,
            // but it's not possible, since we do not have a control over sync execution 
            Assert.That(ParallelContext.GetCurrentContext().IsEmpty, Is.False);

            await Task.Yield();

            Assert.That(ParallelContext.GetCurrentContext().IsEmpty, Is.True);
        }
    }

    [TestCase(1)]
    [TestCase(2)]
    public async Task Await_RegularTaskIsAwaited_ContextIsClearedAndRestored2(int threadsCount)
    {
        await TestBody();

        async ParallelTask TestBody()
        {
            await new ForkingTask(threadsCount);
            var regularAsyncMethod = RegularAsyncMethod();
            await regularAsyncMethod;

            Assert.That(ParallelContext.GetCurrentContext().IsEmpty, Is.False);
            await new JoiningTask();
        }

        async Task RegularAsyncMethod()
        {
            // Note: just explicitly check current behavior. In general, we would like to have an empty context here,
            // but it's not possible, since we do not have a control over sync execution 
            Assert.That(ParallelContext.GetCurrentContext().IsEmpty, Is.False);

            await ParallelMethod();

            Assert.That(ParallelContext.GetCurrentContext().IsEmpty, Is.True);
        }

        async ParallelTask ParallelMethod()
        {
            await new ForkingTask(threadsCount);
            await new JoiningTask();
        }
    }

    [TestCase(1)]
    [TestCase(2)]
    public async Task Await_RegularTaskIsAwaited_ContextIsClearedAndRestored3(int threadsCount)
    {
        await TestBody();

        async ParallelTask TestBody()
        {
            await new ForkingTask(threadsCount);
            var regularAsyncMethod = RegularAsyncMethod();
            await regularAsyncMethod;

            Assert.That(ParallelContext.GetCurrentContext().IsEmpty, Is.False);
            await new JoiningTask();
        }

        async Task RegularAsyncMethod()
        {
            // Note: just explicitly check current behavior. In general, we would like to have an empty context here,
            // but it's not possible, since we do not have a control over sync execution 
            Assert.That(ParallelContext.GetCurrentContext().IsEmpty, Is.False);

            await ParallelMethod();

            Assert.That(ParallelContext.GetCurrentContext().IsEmpty, Is.True);
        }

        async ParallelTask ParallelMethod()
        {
            await new ForkingTask(threadsCount);
            await new JoiningTask();

            try
            {
                await new JoiningTask();
            }
            catch (InvalidOperationException)
            {
            }
        }
    }

    [TestCase(1)]
    [TestCase(2)]
    public async Task Await_RegularTaskIsAwaited_ContextIsClearedAndRestored4(int threadsCount)
    {
        await TestBody();

        async ParallelTask TestBody()
        {
            await new ForkingTask(threadsCount);
            var regularAsyncMethod = RegularAsyncMethod();
            await regularAsyncMethod;

            Assert.That(ParallelContext.GetCurrentContext().IsEmpty, Is.False);
            await new JoiningTask();
        }

        async Task RegularAsyncMethod()
        {
            // Note: just explicitly check current behavior. In general, we would like to have an empty context here,
            // but it's not possible, since we do not have a control over sync execution 
            Assert.That(ParallelContext.GetCurrentContext().IsEmpty, Is.False);

            
            try
            {
                await ParallelMethod();
            }
            catch (InvalidOperationException)
            {
            }

            Assert.That(ParallelContext.GetCurrentContext().IsEmpty, Is.True);
        }

        async ParallelTask ParallelMethod()
        {
            await new ForkingTask(threadsCount);
        }
    }
}