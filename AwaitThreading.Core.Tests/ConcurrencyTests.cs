// MIT License
// Copyright (c) 2024 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

namespace AwaitThreading.Core.Tests;

[TestFixture]
public class ConcurrencyTests : BaseClassWithParallelContextValidation
{
    [Test(
        Description = @"
Tests that there is no race conditions when tasks inside `MethodThatForks` finish
before the calling methods have set the continuation in the corresponding task.
This test should fail if there is an issue with tracking of `RequireContinuationToBeSetBeforeResult` flag.
For example, test should fail after changing the `ForkingAwaiter` to return `false`
in `RequireContinuationToBeSetBeforeResult`.")]
    public async Task NestedOperation_MultipleForks_NoRaceConditions()
    {
        using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        await TestBody().AsTask().WaitAsync(tokenSource.Token);
        return;

        async ParallelTask TestBody()
        {
            for (var i = 0; i < 100; ++i)
            {
                await MethodThatForks();
                await new JoiningTask();
            }
        }

        async ParallelTask MethodThatForks()
        {
            await new ForkingTask(2);
        }
    }

    [Test(Description = "Checks that threads are not blocked after finishing own task")]
    public async Task Fork_ALotOfThreads_NoStarvation()
    {
        using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(1));
        await TestBody().AsTask().WaitAsync(tokenSource.Token);
        return;

        async ParallelTask TestBody()
        {
            await new ForkingTask((Environment.ProcessorCount + 1) * 20);
            await new JoiningTask();
        }
    }
}