// MIT License
// Copyright (c) 2024 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

using System.Collections.Concurrent;
using AwaitThreading.Core.Tasks;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace AwaitThreading.Enumerable.Tests;

[TestFixture]
[TestOf(typeof(CollectionParallelExtensions))]
public class PartitionParallelExtensionsTest
{
    [Test]
    public async Task AsParallelAsync_WithOneThread_IteratesOverAllElementsInNaturalOrder()
    {
        await TestBody();
        return;

        async ParallelTask TestBody()
        {
            var list = System.Linq.Enumerable.Range(0, 10).ToList();
            var result = new ConcurrentBag<int>();

            var partitioner = new NotDynamicPartitioner<int>(list);
            await foreach (var i in await partitioner.AsParallelAsync(1))
            {
                result.Add(i);
            }

            CollectionAssert.AreEquivalent(list, result);
        }
    }

    [TestCase(10, 2)]
    [TestCase(10, 3)]
    [TestCase(100, 2)]
    [TestCase(100, 3)]
    [TestCase(100, 4)]
    [TestCase(100, 5)]
    [TestCase(1, 2)]
    [TestCase(0, 2)]
    public async Task AsParallelAsync_WithDifferentThreadCount_IteratesOverAllElementsOnce(
        int itemsCount,
        int threadsCount)
    {
        await TestBody();
        return;

        async ParallelTask TestBody()
        {
            var list = System.Linq.Enumerable.Range(0, itemsCount).ToList();
            var result = new ConcurrentBag<int>();

            var partitioner = new NotDynamicPartitioner<int>(list);
            await foreach (var i in await partitioner.AsParallelAsync(threadsCount))
            {
                result.Add(i);
            }


            CollectionAssert.AreEquivalent(list, result);
        }
    }

    [Test]
    public async Task AsAsyncParallel_WithOneThread_IteratesOverAllElementsInNaturalOrder()
    {
        await TestBody();
        return;

        async ParallelTask TestBody()
        {
            var list = System.Linq.Enumerable.Range(0, 10).ToList();
            var result = new ConcurrentBag<int>();

            var partitioner = new NotDynamicPartitioner<int>(list);
            await foreach (var i in partitioner.AsAsyncParallel(1))
            {
                result.Add(i);
            }

            CollectionAssert.AreEquivalent(list, result);
        }
    }

    [TestCase(10, 2)]
    [TestCase(10, 3)]
    [TestCase(100, 2)]
    [TestCase(100, 3)]
    [TestCase(100, 4)]
    [TestCase(100, 5)]
    [TestCase(1, 2)]
    [TestCase(0, 2)]
    public async Task AsAsyncParallel_WithDifferentThreadCount_IteratesOverAllElementsOnce(
        int itemsCount,
        int threadsCount)
    {
        await TestBody();
        return;

        async ParallelTask TestBody()
        {
            var list = System.Linq.Enumerable.Range(0, itemsCount).ToList();
            var result = new ConcurrentBag<int>();

            var partitioner = new NotDynamicPartitioner<int>(list);
            await foreach (var i in partitioner.AsAsyncParallel(threadsCount))
            {
                result.Add(i);
            }


            CollectionAssert.AreEquivalent(list, result);
        }
    }

    private class NotDynamicPartitioner<T> : Partitioner<T>
    {
        private readonly IReadOnlyList<T> _list;

        public NotDynamicPartitioner(IReadOnlyList<T> list)
        {
            _list = list;
        }

        public override bool SupportsDynamicPartitions => false;

        public override IList<IEnumerator<T>> GetPartitions(int partitionCount)
         {
             if (partitionCount <= 0)
             {
                 throw new ArgumentOutOfRangeException(nameof(partitionCount));
             }
         
             var partitions = new List<IEnumerator<T>>(partitionCount);
             var partitionSize = _list.Count / partitionCount;
             var remainder = _list.Count % partitionCount;
         
             var currentIndex = 0;
         
             for (var i = 0; i < partitionCount; i++)
             {
                 var currentPartitionSize = partitionSize + (i < remainder ? 1 : 0);
                 partitions.Add(CreatePartitionEnumerator(currentIndex, currentPartitionSize));
                 currentIndex += currentPartitionSize;
             }
         
             return partitions;
         }
         
         private IEnumerator<T> CreatePartitionEnumerator(int start, int count)
         {
             for (var i = 0; i < count; i++)
             {
                 yield return _list[start + i];
             }
         }
    }
}