// MIT License
// Copyright (c) 2024 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

using System.Collections.Concurrent;
using AwaitThreading.Core.Operations;

namespace AwaitThreading.Enumerable;

public sealed class ParallelAsyncLazyForkingPartitionEnumerable<T> : IParallelAsyncLazyForkingEnumerable<T>
{
    private readonly int _threadsCount;
    private readonly ForkingOptions? _forkingOptions;
    private readonly Partitioner<T> _partitioner;

    public ParallelAsyncLazyForkingPartitionEnumerable(Partitioner<T> partitioner, int threadsCount, ForkingOptions? forkingOptions)
    {
        _partitioner = partitioner;
        _threadsCount = threadsCount;
        _forkingOptions = forkingOptions;
    }

    IParallelAsyncLazyForkingEnumerator<T> IParallelAsyncLazyForkingEnumerable<T>.GetAsyncEnumerator()
    {
        return GetAsyncEnumerator();
    }

    public ParallelAsyncLazyForkingPartitionEnumerator<T> GetAsyncEnumerator()
    {
        return new ParallelAsyncLazyForkingPartitionEnumerator<T>(_partitioner, _threadsCount, _forkingOptions);
    }
}