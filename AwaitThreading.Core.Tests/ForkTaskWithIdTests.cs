// MIT License
// Copyright (c) 2025 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

using System.Collections.Concurrent;
using AwaitThreading.Core.Tasks;

namespace AwaitThreading.Core.Tests;

[TestOf(typeof(ForkingTaskWithId))]
public class ForkTaskWithIdTests : BaseClassWithParallelContextValidation
{
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(10)]
    public async Task Fork_NThreadsStarted_NThreadsExecuted(int n)
    {
        var ids = new ConcurrentBag<int>();
        await TestBody();
        Assert.That(ids, Is.EquivalentTo(Enumerable.Range(0, n)));
        return;

        async ParallelTask TestBody()
        {
            var id = await ParallelOperations.Fork(n);
            ids.Add(id);
            await ParallelOperations.Join();
        }
    }

}