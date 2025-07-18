// MIT License
// Copyright (c) 2024 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

using System.Collections.Concurrent;
using AwaitThreading.Core;
using AwaitThreading.Core.Context;
using AwaitThreading.Core.Operations;
using AwaitThreading.Core.Tasks;

namespace AwaitThreading.Enumerable;

public readonly struct ParallelAsyncLazyForkingPartitionEnumerator<T> : IParallelAsyncLazyForkingEnumerator<T>
{
    private readonly Partitioner<T> _partitioner;
    private readonly int _threadCount;
    private readonly ForkingOptions? _forkingOptions;

    // In ideal world we would be able to store enumerator for our chunk in struct field,
    // but any changes to the state of this struct will be lost since async methods are
    // executed on the copy of a struct, so we have to store the data somewhere else.
    private readonly ParallelLocal<IEnumerator<T>> _chunkIndexer = new();

    public ParallelAsyncLazyForkingPartitionEnumerator(Partitioner<T> partitioner, int threadCount, ForkingOptions? forkingOptions)
    {
        _partitioner = partitioner;
        _threadCount = threadCount;
        _forkingOptions = forkingOptions;
    }

    public ParallelValueTask<bool> MoveNextAsync()
    {
        if (_chunkIndexer.IsInitialized)
        {
            return ParallelValueTask.FromResult(_chunkIndexer.Value!.MoveNext());
        }

        return ForkAndMoveNextAsync();
    }

    private async ParallelValueTask<bool> ForkAndMoveNextAsync()
    {
        var partitioner = _partitioner;
        if (partitioner.SupportsDynamicPartitions)
        {
            var dynamicPartitions = partitioner.GetDynamicPartitions();
            await _chunkIndexer.InitializeAndFork(_threadCount, _forkingOptions);

            // ReSharper disable once GenericEnumeratorNotDisposed
            var dynamicEnumerator = dynamicPartitions.GetEnumerator();
            _chunkIndexer.Value = dynamicEnumerator;
            return dynamicEnumerator.MoveNext();
        }

        var partitions = partitioner.GetPartitions(_threadCount);
        await _chunkIndexer.InitializeAndFork(_threadCount, _forkingOptions);

        var enumerator = partitions[ParallelContextStorage.GetTopFrameId()];
        _chunkIndexer.Value = enumerator;
        return enumerator.MoveNext();
    }

    public T Current => _chunkIndexer.Value!.Current;

    public async ParallelTask DisposeAsync()
    {
        if (_chunkIndexer.IsInitialized)
        {
            _chunkIndexer.Value!.Dispose();
            await new JoiningTask();
        }
    }
}