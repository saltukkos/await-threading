//MIT License
//Copyright (c) 2023 Saltuk Konstantin
//See the LICENSE file in the project root for more information.

using AwaitThreading.Core;

namespace AwaitThreading.Enumerable;

public static class ListExtensions
{
    public static async ParallelTask<ChunkEnumerable<T>> AsParallelAsync<T>(this List<T> list, int threadsCount)
    {
        var chunkSize = (list.Count + threadsCount - 1) / threadsCount;
        await new ForkingTask(threadsCount);
        var id = ParallelContext.Id;
        var start = chunkSize * id;
        var end = chunkSize * (id + 1);
        if (end > list.Count)
        {
            end = list.Count;
        }

        return new ChunkEnumerable<T>(list, start, end);
    }
    
    public static ParallelLazyAsyncEnumerable<T> AsParallel<T>(this List<T> list, int threadsCount)
    {
        return new ParallelLazyAsyncEnumerable<T>(list, threadsCount);
    }
}