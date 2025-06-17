// MIT License
// Copyright (c) 2024 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using AwaitThreading.Core.Tasks;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace AwaitThreading.Enumerable.Tests;

[TestFixture]
[TestOf(typeof(CollectionParallelExtensions))]
public class CollectionParallelExtensionsTest
{
    [Test]
    public async Task AsParallelAsync_WithOneThread_IteratesOverAllElementsInNaturalOrder()
    {
        await TestBody();
        return;

        async ParallelTask TestBody()
        {
            var list = System.Linq.Enumerable.Range(0, 10).ToList();
            var result = new List<int>();
            await foreach (var i in await list.AsParallelAsync(1))
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
            await foreach (var i in await list.AsParallelAsync(threadsCount))
            {
                result.Add(i);
            }

            CollectionAssert.AreEquivalent(list, result);
        }
    }
    
    [Test]
    public async Task AsParallelLazyAsync_WithOneThread_IteratesOverAllElementsInNaturalOrder()
    {
        await TestBody();
        return;

        async ParallelTask TestBody()
        {
            var list = System.Linq.Enumerable.Range(0, 10).ToList();
            var result = new List<int>();
            await foreach (var i in list.AsAsyncParallel(1))
            {
                result.Add(i);
            }

            CollectionAssert.AreEquivalent(list, result);
        }
    }

    [TestCase(10, 1)]
    [TestCase(10, 2)]
    [TestCase(10, 3)]
    [TestCase(100, 2)]
    [TestCase(100, 3)]
    [TestCase(100, 4)]
    [TestCase(100, 5)]
    [TestCase(1, 2)]
    [TestCase(0, 2)]
    public async Task AsParallelLazyAsync_WithDifferentThreadCount_IteratesOverAllElementsOnce(
        int itemsCount,
        int threadsCount)
    {
        await TestBody();
        return;

        async ParallelTask TestBody()
        {
            var list = System.Linq.Enumerable.Range(0, itemsCount).ToList();
            var result = new ConcurrentBag<int>();
            await foreach (var i in list.AsAsyncParallel(threadsCount))
            {
                result.Add(i);
            }

            CollectionAssert.AreEquivalent(list, result);
        }
    }

    [Test]
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public async Task AsParallelAsyncEnumerable_WithOneThread_IteratesOverAllElementsInNaturalOrder()
    {
        await TestBody();
        return;

        async ParallelTask TestBody()
        {
            var list = System.Linq.Enumerable.Range(0, 10);
            var result = new List<int>();
            await foreach (var i in await list.AsParallelAsync(1))
            {
                result.Add(i);
            }

            CollectionAssert.AreEquivalent(list, result);
        }
    }

    [TestCase(10, 1)]
    [TestCase(10, 2)]
    [TestCase(10, 3)]
    [TestCase(100, 2)]
    [TestCase(100, 3)]
    [TestCase(100, 4)]
    [TestCase(100, 5)]
    [TestCase(1, 2)]
    [TestCase(0, 2)]
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public async Task AsParallelAsyncEnumerable_WithDifferentThreadCount_IteratesOverAllElementsOnce(
        int itemsCount,
        int threadsCount)
    {
        await TestBody();
        return;

        async ParallelTask TestBody()
        {
            var list = System.Linq.Enumerable.Range(0, itemsCount);
            var result = new ConcurrentBag<int>();
            await foreach (var i in await list.AsParallelAsync(threadsCount))
            {
                result.Add(i);
            }

            CollectionAssert.AreEquivalent(list, result);
        }
    }

    [Test]
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public async Task AsAsyncParallelEnumerable_WithOneThread_IteratesOverAllElementsInNaturalOrder()
    {
        await TestBody();
        return;

        async ParallelTask TestBody()
        {
            var list = System.Linq.Enumerable.Range(0, 10);
            var result = new List<int>();
            await foreach (var i in list.AsAsyncParallel(1))
            {
                result.Add(i);
            }

            CollectionAssert.AreEquivalent(list, result);
        }
    }

    [TestCase(10, 1)]
    [TestCase(10, 2)]
    [TestCase(10, 3)]
    [TestCase(100, 2)]
    [TestCase(100, 3)]
    [TestCase(100, 4)]
    [TestCase(100, 5)]
    [TestCase(1, 2)]
    [TestCase(0, 2)]
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public async Task AsAsyncParallelEnumerable_WithDifferentThreadCount_IteratesOverAllElementsOnce(
        int itemsCount,
        int threadsCount)
    {
        await TestBody();
        return;

        async ParallelTask TestBody()
        {
            var list = System.Linq.Enumerable.Range(0, itemsCount);
            var result = new ConcurrentBag<int>();
            await foreach (var i in list.AsAsyncParallel(threadsCount))
            {
                result.Add(i);
            }

            CollectionAssert.AreEquivalent(list, result);
        }
    }

    [Test]
    public async Task AsParallelAsync_WithZeroThreads_ThrowsException()
    {
        await TestBody();
        return;

        async ParallelTask TestBody()
        {
            var gotException = false;
            try
            {
                var list = System.Linq.Enumerable.Range(0, 10).ToList();
                await foreach (var unused in await list.AsParallelAsync(0))
                {
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                gotException = true;
            }

            Assert.That(gotException, Is.True);
        }
    }
}