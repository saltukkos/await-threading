//MIT License
//Copyright (c) 2023 Saltuk Konstantin
//See the LICENSE file in the project root for more information.

using AwaitThreading.Core;

namespace AwaitThreading.Enumerable;

public static class ListExtensions
{
    public static async ParallelTask<ParallelAsyncEnumerable<T>> AsParallelAsync<T>(this List<T> list, int threadsCount)
    {
        Logger.Log("inside AsParallelAsync before await");
        await new ForkingTask(threadsCount);
        Logger.Log("inside AsParallelAsync after await");
        var context = ParallelContext.GetCurrentFrame();
        var id = context.Id;
        var chunkSize = (list.Count + threadsCount - 1) / threadsCount;
        var start = chunkSize * id;
        var end = chunkSize * (id + 1);
        if (end > list.Count)
        {
            end = list.Count;
        }

        var part = new List<T>(list.Skip(start).Take(end - start));
        return new ParallelAsyncEnumerable<T>(part);
    }
    
    public static ParallelLazyAsyncEnumerable<T> AsParallel<T>(this List<T> list, int threadsCount)
    {
        return new ParallelLazyAsyncEnumerable<T>(list, threadsCount);
    }
}