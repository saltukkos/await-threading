// MIT License
// Copyright (c) 2024 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

using System.Collections.Concurrent;
using AwaitThreading.Core.Operations;

namespace AwaitThreading.Enumerable;

public static partial class CollectionParallelExtensions
{
    public static ParallelAsyncLazyForkingRangeEnumerable<T> AsAsyncParallel<T>(
        this IReadOnlyList<T> list,
        int threadsCount,
        ForkingOptions? forkingOptions = null)
    {
        return new ParallelAsyncLazyForkingRangeEnumerable<T>(list, threadsCount, forkingOptions);
    }

    public static ParallelAsyncLazyForkingPartitionEnumerable<T> AsAsyncParallel<T>(
        this Partitioner<T> partitioner,
        int threadsCount,
        ForkingOptions? forkingOptions = null)
    {
        return new ParallelAsyncLazyForkingPartitionEnumerable<T>(partitioner, threadsCount, forkingOptions);
    }

    public static IParallelAsyncLazyForkingEnumerable<T> AsAsyncParallel<T>(
        this IEnumerable<T> enumerable,
        int threadsCount, 
        ForkingOptions? forkingOptions = null)
    {
        return enumerable switch
        {
            IReadOnlyList<T> list => new ParallelAsyncLazyForkingRangeEnumerable<T>(list, threadsCount, forkingOptions),
            _ => new ParallelAsyncLazyForkingPartitionEnumerable<T>(Partitioner.Create(enumerable), threadsCount, forkingOptions)
        };
    }
}