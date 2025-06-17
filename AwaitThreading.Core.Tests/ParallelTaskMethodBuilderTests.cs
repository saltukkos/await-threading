// MIT License
// Copyright (c) 2024 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

using AwaitThreading.Core.Context;
using AwaitThreading.Core.Operations;
using AwaitThreading.Core.Tasks;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
namespace AwaitThreading.Core.Tests;

[TestFixture]
[TestOf(typeof(ParallelTaskMethodBuilder))]
[TestOf(typeof(ParallelTaskMethodBuilder<>))]
public class ParallelTaskMethodBuilderTests : BaseClassWithParallelContextValidation
{
    [Test]
    public async Task Await_VoidResultSetSync_ResultReturned()
    {
        await TestBody();
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
        var result = await TestBody();
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
        var result = await TestBody();
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
        var result = await TestBody();
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
        var result = await TestBody();
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
            return ParallelContextStorage.GetTopFrameId() == 0 ? 1 : 2;
        }
    }

    [Test]
    public async Task AwaitVoid_ExceptionIsThrownInSyncContext_ExceptionIsPropagated()
    {
        await AssertEx.CheckThrowsAsync<ArgumentOutOfRangeException>(() => TestBody().AsTask());
        return;

        async ParallelTask TestBody()
        {
            throw new ArgumentOutOfRangeException();
        }
    }

    [Test]
    public async Task AwaitWithResult_ExceptionIsThrownInSyncContext_ExceptionIsPropagated()
    {
        await AssertEx.CheckThrowsAsync<ArgumentOutOfRangeException>(() => TestBody().AsTask());
        return;

        async ParallelTask<int> TestBody()
        {
            throw new ArgumentOutOfRangeException();
        }
    }

    [Test]
    public async Task AwaitVoid_ExceptionIsThrownInSubMethodSync_ExceptionIsPropagated()
    {
        await AssertEx.CheckThrowsAsync<ArgumentOutOfRangeException>(() => TestBody().AsTask());
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
        await AssertEx.CheckThrowsAsync<ArgumentOutOfRangeException>(() => TestBody().AsTask());
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
        await AssertEx.CheckThrowsAsync<ArgumentOutOfRangeException>(() => TestBody().AsTask());
        return;

        async ParallelTask<int> TestBody()
        {
            await new ForkingTask(2);
            await new JoiningTask();
            throw new ArgumentOutOfRangeException();
        }
    }

    [Test]
    public async Task Await_MultipleCallsOfParallelMethod_StackTrackDoNotGrow()
    {
        await TestBody();
        return;

        async ParallelTask TestBody()
        {
            await DoSomething();
            var stackTrace1 = Environment.StackTrace;
            await DoSomething();
            var stackTrace2 = Environment.StackTrace;

            Assert.That(stackTrace1, Is.EqualTo(stackTrace2).UsingStringLinesCountEquality());
        }

        async ParallelTask DoSomething()
        {
            await new ForkingTask(2);
            await new JoiningTask();
        }
    }

    [Test]
    public async Task Await_MultipleForks_StackTrackDoNotGrow()
    {
        await TestBody();
        return;

        async ParallelTask TestBody()
        {
            await new ForkingTask(2);
            var stackTrace1 = Environment.StackTrace;
            await new ForkingTask(2);
            var stackTrace2 = Environment.StackTrace;
            await new JoiningTask();
            await new JoiningTask();

            Assert.That(stackTrace1, Is.EqualTo(stackTrace2).UsingStringLinesCountEquality());
        }
    }

    [Test]
    public async Task Await_MultipleCompleted_StackTrackDoNotGrow()
    {
        await TestBody();
        return;

        async ParallelTask TestBody()
        {
            await GetResult();
            var stackTrace1 = Environment.StackTrace;
            await GetResult();
            var stackTrace2 = Environment.StackTrace;

            Assert.That(stackTrace1, Is.EqualTo(stackTrace2).UsingStringLinesCountEquality());
        }

        async ParallelTask<int> GetResult()
        {
            return 42;
        }
    }

    [Test, /*Ignore("This test can also fail with standard `Task<T>`")*/]
    public async Task Await_MultipleNotCompleted_StackTrackDoNotGrow()
    {
        await TestBody();
        return;

        async ParallelTask TestBody()
        {
            await GetResult();
            var stackTrace1 = Environment.StackTrace;
            await GetResult();
            var stackTrace2 = Environment.StackTrace;

            Assert.That(stackTrace1.Split('\n').Length, Is.EqualTo(stackTrace2.Split('\n').Length));
        }

        async ParallelTask<int> GetResult()
        {
            await Task.Yield();
            return 42;
        }
    }
    
    [Test]
    public async Task Await_MultipleCallsOfNestedParallelMethod_StackTrackDoNotGrow()
    {
        await TestBody();
        return;

        async ParallelTask TestBody()
        {
            await DoSomething();
            var stackTrace1 = Environment.StackTrace;
            await DoSomething();
            var stackTrace2 = Environment.StackTrace;

            Assert.That(stackTrace1, Is.EqualTo(stackTrace2).UsingStringLinesCountEquality());
        }

        async ParallelTask DoSomething()
        {
            await DoSomethingNested();
        }
        
        async ParallelTask DoSomethingNested()
        {
            await new ForkingTask(2);
            await new JoiningTask();
        }
    }
}