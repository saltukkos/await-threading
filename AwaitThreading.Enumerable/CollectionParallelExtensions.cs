//MIT License
//Copyright (c) 2023 Saltuk Konstantin
//See the LICENSE file in the project root for more information.

using AwaitThreading.Core;

namespace AwaitThreading.Enumerable;

public static class CollectionParallelExtensions
{
    public static async ParallelTask<ParallelAsyncEnumerable<T>> AsParallelAsync<T>(this IReadOnlyList<T> list, int threadsCount)
    {
        var rangeManager = new RangeManager(0, list.Count, 1, threadsCount);
        await new ForkingTask(threadsCount);
        return new ParallelAsyncEnumerable<T>(list, rangeManager.RegisterNewWorker());
    }
    
    public static ParallelAsyncLazyForkingEnumerable<T> AsAsyncParallel<T>(this IReadOnlyList<T> list, int threadsCount)
    {
        return new ParallelAsyncLazyForkingEnumerable<T>(list, threadsCount);
    }
}