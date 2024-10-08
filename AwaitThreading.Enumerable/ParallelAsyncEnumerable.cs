//MIT License
//Copyright (c) 2023 Saltuk Konstantin
//See the LICENSE file in the project root for more information.

namespace AwaitThreading.Enumerable;

public readonly struct ChunkEnumerable<T>
{
    private readonly List<T> _list;
    private readonly RangeWorker _rangeWorker;

    internal ChunkEnumerable(List<T> list, RangeWorker rangeWorker)
    {
        _list = list;
        _rangeWorker = rangeWorker;
    }
    
    public ChunkEnumerator<T> GetAsyncEnumerator()
    {
        return new ChunkEnumerator<T>(_list, _rangeWorker);
    }
}