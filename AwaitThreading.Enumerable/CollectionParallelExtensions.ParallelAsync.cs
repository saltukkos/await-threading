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
        int threadsCount)
    {
        var rangeManager = new RangeManager(0, list.Count, 1, threadsCount);
        await new ForkingTask(threadsCount);
        return new ParallelAsyncEnumerable<T>(list, rangeManager.RegisterNewWorker());
    }

    public static async ParallelTask<IParallelAsyncEnumerable<T>> AsParallelAsync<T>(
        this Partitioner<T> partitioner,
        int threadsCount)
    {
        var forked = false;
        try
        {
            if (partitioner.SupportsDynamicPartitions)
            {
                var dynamicPartitions = partitioner.GetDynamicPartitions();
                await new ForkingTask(threadsCount);
                forked = true;

                return new ParallelAsyncDelegatingEnumerable<T>(dynamicPartitions.GetEnumerator());
            }

            var partitions = partitioner.GetPartitions(threadsCount);
            await new ForkingTask(threadsCount);
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
        int threadsCount)
    {
        return enumerable switch
        {
            IReadOnlyList<T> list => AsParallelAsyncBoxed(list, threadsCount),
            _ => AsParallelAsync(Partitioner.Create(enumerable), threadsCount)
        };
    }

    private static async ParallelTask<IParallelAsyncEnumerable<T>> AsParallelAsyncBoxed<T>(
        IReadOnlyList<T> list,
        int threadsCount)
    {
        var rangeManager = new RangeManager(0, list.Count, 1, threadsCount);
        await new ForkingTask(threadsCount);
        return new ParallelAsyncEnumerable<T>(list, rangeManager.RegisterNewWorker());
    }

}