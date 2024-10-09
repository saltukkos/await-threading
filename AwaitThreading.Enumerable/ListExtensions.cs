//MIT License
//Copyright (c) 2023 Saltuk Konstantin
//See the LICENSE file in the project root for more information.

using AwaitThreading.Core;

namespace AwaitThreading.Enumerable;

public static class ListExtensions
{
    public static async ParallelTask<ChunkEnumerable<T>> AsParallelAsync<T>(this List<T> list, int threadsCount)
    {
        var rangeManager = new RangeManager(0, list.Count, 1, threadsCount);
        await new ForkingTask(threadsCount);
        return new ChunkEnumerable<T>(list, rangeManager.RegisterNewWorker());
    }
    
    public static ParallelLazyAsyncEnumerable<T> AsParallel<T>(this List<T> list, int threadsCount)
    {
        return new ParallelLazyAsyncEnumerable<T>(list, threadsCount);
    }
}