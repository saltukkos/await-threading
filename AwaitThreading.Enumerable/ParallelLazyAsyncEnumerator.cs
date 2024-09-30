//MIT License
//Copyright (c) 2023 Saltuk Konstantin
//See the LICENSE file in the project root for more information.

using AwaitThreading.Core;
using JetBrains.Annotations;

namespace AwaitThreading.Enumerable;

public readonly struct ParallelLazyAsyncEnumerator<T>
{
    private struct ChunkIndexer
    {
        private readonly int _maxIndex;
        private int _currentIndex;

        public ChunkIndexer(int startIndex, int endIndex)
        {
            _currentIndex = startIndex - 1;
            _maxIndex = endIndex - 1;
        }

        public bool MoveNext()
        {
            if (_currentIndex >= _maxIndex)
            {
                return false;
            }
            
            _currentIndex++;
            return true;
        }

        public T GetItem(List<T> list) => list[_currentIndex];
    }
    
    private readonly List<T> _list;
    private readonly int _threadsCount;

    // In ideal world we would be able to store enumerator for our chunk in struct field,
    // but any changes to the state of this struct will be lost since async methods are
    // executed on the copy of a struct, so we have to store the data somewhere else.
    private readonly ParallelLocal<ChunkIndexer> _chunkIndexer = new();

    public ParallelLazyAsyncEnumerator(List<T> list, int threadsCount)
    {
        _threadsCount = threadsCount;
        _list = list;
    }

    public ParallelValueTask<bool> MoveNextAsync()
    {
        if (_chunkIndexer.IsInitialized)
        {
            return ParallelValueTask.FromResult(_chunkIndexer.Value.MoveNext());
        }

        return ForkAndMoveNextAsync();
    }

    private async ParallelValueTask<bool> ForkAndMoveNextAsync()
    {
        await _chunkIndexer.InitializeAndFork(_threadsCount);
        var id = ParallelContext.Id;

        var count = _list.Count;
        var chunkSize = (count + _threadsCount - 1) / _threadsCount;
        var start = chunkSize * id;
        var end = Math.Min(chunkSize * (id + 1), count);

        var indexer = new ChunkIndexer(start, end);
        var returnResul = indexer.MoveNext();
        _chunkIndexer.Value = indexer;
        return returnResul;
    }

    public T Current => _chunkIndexer.Value.GetItem(_list);

    [UsedImplicitly] //TODO: detect in usage analysis
    public async ParallelTask DisposeAsync()
    {
        await new JoiningTask();
    }
}