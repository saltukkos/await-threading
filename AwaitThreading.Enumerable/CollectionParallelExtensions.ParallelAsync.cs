//MIT License
//Copyright (c) 2023 Saltuk Konstantin
//See the LICENSE file in the project root for more information.

using System.Collections.Concurrent;
using AwaitThreading.Core.Context;
using AwaitThreading.Core.Operations;
using AwaitThreading.Core.Tasks;

namespace AwaitThreading.Enumerable;

public static partial class CollectionParallelExtensions
{
    public static async ParallelTask<ParallelAsyncEnumerable<T>> AsParallelAsync<T>(
        this IReadOnlyList<T> list,
        int threadCount,
        ForkingOptions? forkingOptions = null)
    {
        var rangeManager = new RangeManager(0, list.Count, 1, threadCount);
        await new ForkingTask(threadCount, forkingOptions);
        return new ParallelAsyncEnumerable<T>(list, rangeManager.RegisterNewWorker());
    }

    public static async ParallelTask<IParallelAsyncEnumerable<T>> AsParallelAsync<T>(
        this Partitioner<T> partitioner,
        int threadCount,
        ForkingOptions? forkingOptions = null)
    {
        var forked = false;
        try
        {
            if (partitioner.SupportsDynamicPartitions)
            {
                var dynamicPartitions = partitioner.GetDynamicPartitions();
                await new ForkingTask(threadCount, forkingOptions);
                forked = true;

                return new ParallelAsyncDelegatingEnumerable<T>(dynamicPartitions.GetEnumerator());
            }

            var partitions = partitioner.GetPartitions(threadCount);
            await new ForkingTask(threadCount, forkingOptions);
            forked = true;

            return new ParallelAsyncDelegatingEnumerable<T>(partitions[ParallelContextStorage.GetTopFrameId()]);
        }
        catch
        {
            if (forked)
            {
                await new JoiningTask();
            }

            throw;
        }
    }

    public static ParallelTask<IParallelAsyncEnumerable<T>> AsParallelAsync<T>(
        this IEnumerable<T> enumerable,
        int threadCount,
        ForkingOptions? forkingOptions = null)
    {
        return enumerable switch
        {
            IReadOnlyList<T> list => AsParallelAsyncBoxed(list, threadCount, forkingOptions),
            _ => AsParallelAsync(Partitioner.Create(enumerable), threadCount)
        };
    }

    private static async ParallelTask<IParallelAsyncEnumerable<T>> AsParallelAsyncBoxed<T>(
        IReadOnlyList<T> list,
        int threadCount,
        ForkingOptions? forkingOptions)
    {
        var rangeManager = new RangeManager(0, list.Count, 1, threadCount);
        await new ForkingTask(threadCount, forkingOptions);
        return new ParallelAsyncEnumerable<T>(list, rangeManager.RegisterNewWorker());
    }

}