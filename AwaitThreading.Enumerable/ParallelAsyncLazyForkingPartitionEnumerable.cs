// MIT License
// Copyright (c) 2024 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

using System.Collections.Concurrent;

namespace AwaitThreading.Enumerable;

public sealed class ParallelAsyncLazyForkingPartitionEnumerable<T> : IParallelAsyncLazyForkingEnumerable<T>
{
    private readonly int _threadsCount;
    private readonly Partitioner<T> _partitioner;

    public ParallelAsyncLazyForkingPartitionEnumerable(Partitioner<T> partitioner, int threadsCount)
    {
        _partitioner = partitioner;
        _threadsCount = threadsCount;
    }

    IParallelAsyncLazyForkingEnumerator<T> IParallelAsyncLazyForkingEnumerable<T>.GetAsyncEnumerator()
    {
        return GetAsyncEnumerator();
    }

    public ParallelAsyncLazyForkingPartitionEnumerator<T> GetAsyncEnumerator()
    {
        return new ParallelAsyncLazyForkingPartitionEnumerator<T>(_partitioner, _threadsCount);
    }
}