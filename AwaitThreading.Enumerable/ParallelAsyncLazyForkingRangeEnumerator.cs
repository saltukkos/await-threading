//MIT License
//Copyright (c) 2023 Saltuk Konstantin
//See the LICENSE file in the project root for more information.

using AwaitThreading.Core;
using AwaitThreading.Core.Operations;
using AwaitThreading.Core.Tasks;

namespace AwaitThreading.Enumerable;

public readonly struct ParallelAsyncLazyForkingRangeEnumerator<T> : IParallelAsyncLazyForkingEnumerator<T>
{
    private class RangeEnumerator
    {
        private RangeWorker _rangeWorker;
        private int _fromInclusive;
        private int _toExclusive;

        public RangeEnumerator(RangeWorker rangeWorker)
        {
            _rangeWorker = rangeWorker;
        }

        public bool MoveNext()
        {
            if (_fromInclusive++ >= _toExclusive - 1)
            {
                return _rangeWorker.FindNewWork(out _fromInclusive, out _toExclusive);
            }
            
            return true;
        }

        public T GetItem(IReadOnlyList<T> list) => list[_fromInclusive];
    }
    
    private readonly IReadOnlyList<T> _list;
    private readonly int _threadCount;
    private readonly ForkingOptions? _forkingOptions;

    // In ideal world we would be able to store enumerator for our chunk in struct field,
    // but any changes to the state of this struct will be lost since async methods are
    // executed on the copy of a struct, so we have to store the data somewhere else.
    private readonly ParallelLocal<RangeEnumerator> _chunkIndexer = new();

    public ParallelAsyncLazyForkingRangeEnumerator(IReadOnlyList<T> list, int threadCount, ForkingOptions? forkingOptions)
    {
        _threadCount = threadCount;
        _forkingOptions = forkingOptions;
        _list = list;
    }

    public ParallelValueTask<bool> MoveNextAsync()
    {
        if (_chunkIndexer.IsInitialized)
        {
            return ParallelValueTask.FromResult(_chunkIndexer.Value!.MoveNext());
        }

        if (_list.Count == 0)
        {
            return ParallelValueTask.FromResult(false);
        }

        return ForkAndMoveNextAsync();
    }

    private async ParallelValueTask<bool> ForkAndMoveNextAsync()
    {
        var rangeManager = new RangeManager(0, _list.Count, 1, _threadCount);
        await _chunkIndexer.InitializeAndFork(_threadCount, _forkingOptions);
        var indexer = new RangeEnumerator(rangeManager.RegisterNewWorker());
        var returnResult = indexer.MoveNext();
        _chunkIndexer.Value = indexer;
        return returnResult;
    }

    public T Current => _chunkIndexer.Value!.GetItem(_list);

    public async ParallelTask DisposeAsync()
    {
        if (_chunkIndexer.IsInitialized)
        {
            await new JoiningTask();
        }
    }
}