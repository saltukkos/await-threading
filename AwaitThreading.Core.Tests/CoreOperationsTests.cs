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
        await TestBody().WaitAsync();
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
        await TestBody().WaitAsync();
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
        await TestBody().WaitAsync();
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
        await TestBody().WaitAsync();
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
    
    //TODO: shared resources tests
    
    //TODO: race conditions tests (like in samples)
}