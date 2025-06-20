// MIT License
// Copyright (c) 2025 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

using AwaitThreading.Core.Tasks;
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace AwaitThreading.Core.Tests.Tasks;

[TestFixture]
[TestOf(typeof(ParallelValueTask))]
[TestOf(typeof(ParallelValueTask<>))]
public class ParallelValueTaskTest
{
    [Test]
    public void SyncVoidResult_IsCompleted_True()
    {
        var parallelValueTask = ParallelValueTask.CompletedTask;
        Assert.That(parallelValueTask.GetAwaiter().IsCompleted, Is.True);
        Assert.That(parallelValueTask.GetAwaiter().GetResult, Throws.Nothing);
    }

    [Test]
    public void SyncResult_GetResult_ReturnsResult()
    {
        var parallelValueTask = ParallelValueTask.FromResult(42);
        Assert.That(parallelValueTask.GetAwaiter().IsCompleted, Is.True);
        Assert.That(parallelValueTask.GetAwaiter().GetResult(), Is.EqualTo(42));
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task AsyncVoidResult_Await_DoesNotThrow(bool useParallelOperations)
    {
        await ParallelMethod();

        async ParallelValueTask ParallelMethod()
        {
            if (useParallelOperations)
            {
                await ParallelOperations.Fork(2);
                await ParallelOperations.Join();
            }
            else
            {
                await Task.Yield();
            }
        }
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task AsyncException_Await_ThrowsException(bool useParallelOperations)
    {
        await AssertEx.CheckThrowsAsync<ArgumentOutOfRangeException>(async () => await ParallelMethod());

        async ParallelValueTask ParallelMethod()
        {
            if (useParallelOperations)
            {
                await ParallelOperations.Fork(2);
                await ParallelOperations.Join();
            }
            else
            {
                await Task.Yield();
            }

            throw new ArgumentOutOfRangeException();
        }
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task AsyncResult_Await_ReturnsResult(bool useParallelOperations)
    {
        var result = await ParallelMethod();
        Assert.That(result, Is.EqualTo(42));

        async ParallelValueTask<int> ParallelMethod()
        {
            if (useParallelOperations)
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
    public async Task AsyncWithResultException_Await_ThrowsException(bool useParallelOperations)
    {
        await AssertEx.CheckThrowsAsync<ArgumentOutOfRangeException>(async () => await ParallelMethod());

        async ParallelValueTask<int> ParallelMethod()
        {
            if (useParallelOperations)
            {
                await ParallelOperations.Fork(2);
                await ParallelOperations.Join();
            }
            else
            {
                await Task.Yield();
            }

            throw new ArgumentOutOfRangeException();
        }
    }
    
    [Test]
    public void AsyncVoidResultWithoutAwaits_IsCompleted_False()
    {
        var parallelValueTask = ParallelMethod();
        Assert.That(parallelValueTask.GetAwaiter().IsCompleted, Is.False);
        Assert.That(parallelValueTask.GetAwaiter().GetResult, Throws.TypeOf<NotSupportedException>());

        async ParallelValueTask ParallelMethod()
        {
        }
    }

    [Test]
    public void AsyncResultWithoutAwaits_IsCompleted_False()
    {
        var parallelValueTask = ParallelMethod();
        Assert.That(parallelValueTask.GetAwaiter().IsCompleted, Is.False);
        Assert.That(parallelValueTask.GetAwaiter().GetResult, Throws.TypeOf<NotSupportedException>());

        async ParallelValueTask<int> ParallelMethod()
        {
            return 42;
        }
    }

    [Test]
    public async Task AsyncVoidResultWithoutAwaits_Await_DoesNotThrow()
    {
        await ParallelMethod();

        async ParallelValueTask ParallelMethod()
        {
        }
    }

    [Test]
    public async Task AsyncResultWithoutAwaits_Await_ReturnsResult()
    {
        var result = await ParallelMethod();
        Assert.That(result, Is.EqualTo(42));

        async ParallelValueTask<int> ParallelMethod()
        {
            return 42;
        }
    }

    [Test]
    public async Task MethodsComposition_AwaitedFromParallelTask_ParallelOperationsWork()
    {
        await TestBody();

        async ParallelValueTask TestBody()
        {
            var parallelCounter = new ParallelCounter();
            await ForkingMethod();
            parallelCounter.Increment();
            await JoiningMethod();

            Assert.That(parallelCounter.Count, Is.EqualTo(2));
        }

        async ParallelValueTask<int> ForkingMethod()
        {
            await ParallelOperations.Fork(2);
            return 42;
        }

        async ParallelValueTask JoiningMethod()
        {
            await ParallelOperations.Join();
        }
    }
}