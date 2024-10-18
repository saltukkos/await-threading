// MIT License
// Copyright (c) 2024 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

using System.Collections.Concurrent;

namespace AwaitThreading.Enumerable;

public static partial class CollectionParallelExtensions
{
    public static ParallelAsyncLazyForkingRangeEnumerable<T> AsAsyncParallel<T>(
        this IReadOnlyList<T> list,
        int threadsCount)
    {
        return new ParallelAsyncLazyForkingRangeEnumerable<T>(list, threadsCount);
    }

    public static ParallelAsyncLazyForkingPartitionEnumerable<T> AsAsyncParallel<T>(
        this Partitioner<T> partitioner,
        int threadsCount)
    {
        return new ParallelAsyncLazyForkingPartitionEnumerable<T>(partitioner, threadsCount);
    }

    public static IParallelAsyncLazyForkingEnumerable<T> AsAsyncParallel<T>(
        this IEnumerable<T> enumerable,
        int threadsCount)
    {
        return enumerable switch
        {
            IReadOnlyList<T> list => new ParallelAsyncLazyForkingRangeEnumerable<T>(list, threadsCount),
            _ => new ParallelAsyncLazyForkingPartitionEnumerable<T>(Partitioner.Create(enumerable), threadsCount)
        };
    }
}