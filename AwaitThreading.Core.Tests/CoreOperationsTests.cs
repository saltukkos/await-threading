// MIT License
// Copyright (c) 2024 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

namespace AwaitThreading.Core.Tests;

[TestFixture]
[TestOf(typeof(ForkingTask))]
[TestOf(typeof(JoiningTask))]
public class CoreOperationsTests
{
    [Test]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(10)]
    public async Task Fork_NThreadsStarted_NThreadsExecuted(int n)
    {
        var counter = new ParallelCounter();
        await TestBody().AsTask();
        counter.AssertCount(n);
        return;

        async ParallelTask TestBody()
        {
            await new ForkingTask(n);
            counter.Increment();
            await new JoiningTask();
        }
    }

    [Test]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    public async Task NestedOperation_Fork_ForksAreMultiplied(int n)
    {
        var counter = new ParallelCounter();
        await TestBody().AsTask();
        counter.AssertCount(n * n);
        return;

        async ParallelTask TestBody()
        {
            await new ForkingTask(n);
            await NestedFork();
            counter.Increment();
            await new JoiningTask();
            await new JoiningTask();
        }

        async ParallelTask NestedFork()
        {
            await new ForkingTask(n);
        }
    }

    [Test]
    public async Task NestedOperation_Join_JoinAffectsExternalMethod()
    {
        var counter = new ParallelCounter();
        await TestBody().AsTask();
        counter.AssertCount(1);
        return;

        async ParallelTask TestBody()
        {
            await new ForkingTask(2);
            await NestedJoin();
            counter.Increment();
        }

        async ParallelTask NestedJoin()
        {
            await new JoiningTask();
        }
    }

    [Test]
    public async Task NestedOperation_MultipleNested_AffectsExternalMethod()
    {
        var counterAfterForks = new ParallelCounter();
        var counterAfterJoins = new ParallelCounter();
        await TestBody().AsTask();
        counterAfterForks.AssertCount(4);
        counterAfterJoins.AssertCount(1);
        return;

        async ParallelTask TestBody()
        {
            await NestedFork1();
            counterAfterForks.Increment();
            await NestedJoin1();
            counterAfterJoins.Increment();
        }

        async ParallelTask NestedFork1()
        {
            await new ForkingTask(2);
            await NestedFork2();
        }

        async ParallelTask NestedFork2()
        {
            await new ForkingTask(2);
        }

        async ParallelTask NestedJoin1()
        {
            await new JoiningTask();
            await NestedJoin2();
        }

        async ParallelTask NestedJoin2()
        {
            await new JoiningTask();
        }
    }

    [Test]
    public async Task SharedState_ReferencesCreatedBeforeFork_SameReferenceAfterFork()
    {
        var res = await TestBody().AsTask();
        Assert.That(res[0], Is.EqualTo(1));
        Assert.That(res[1], Is.EqualTo(1));
        return;

        async ParallelTask<int[]> TestBody()
        {
            var sharedArray = new int[2];
            await new ForkingTask(2);
            sharedArray[ParallelContext.Id] = 1;
            await new JoiningTask();
            return sharedArray;
        }
    }

    [Test]
    public async Task SharedState_ReferencesCreatedAfterFork_ReferencesAreDifferent()
    {
        var res = await TestBody().AsTask();
        Assert.That(res[0], Is.Not.SameAs(res[1]));
        return;

        async ParallelTask<object[]> TestBody()
        {
            var sharedArray = new object[2];
            await new ForkingTask(2);
            var localObject = new object();
            sharedArray[ParallelContext.Id] = localObject;
            await new JoiningTask();
            return sharedArray;
        }
    }

    [Test]
    public async Task SharedStateWithTargeredJoin_ReferencesCreatedAfterFork_ReferenceFromThread0IsAvailableAfterJoin()
    {
        var res = await TestBody().AsTask();
        Assert.That(res.SharedArray[0], Is.SameAs(res.ValueAfterJoin));
        return;

        async ParallelTask<(object[] SharedArray, object ValueAfterJoin)> TestBody()
        {
            var sharedArray = new object[2];
            await new ForkingTask(2);
            var localObject = new object();
            sharedArray[ParallelContext.Id] = localObject;
            await new TargetedJoiningTask();
            return (sharedArray, localObject);
        }
    }

    [Test]
    public async Task SharedState_ValueTypeIsDefinedBeforeFork_ChangedSeparately()
    {
        var res = await TestBody().AsTask();
        Assert.That(res.SharedArray[0], Is.EqualTo(2));
        Assert.That(res.SharedArray[1], Is.EqualTo(2));
        Assert.That(res.ValueAfterJoin, Is.EqualTo(2));
        return;

        async ParallelTask<(int[] SharedArray, object ValueAfterJoin)> TestBody()
        {
            var sharedArray = new int[2];
            var sharedInt = 1;

            await new ForkingTask(2);
            var incrementedValue = Interlocked.Increment(ref sharedInt);
            sharedArray[ParallelContext.Id] = incrementedValue;
            await new JoiningTask();
            
            return (sharedArray, sharedInt);
        }
    }
}