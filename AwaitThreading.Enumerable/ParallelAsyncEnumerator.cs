//MIT License
//Copyright (c) 2023 Saltuk Konstantin
//See the LICENSE file in the project root for more information.

using AwaitThreading.Core;
using JetBrains.Annotations;

namespace AwaitThreading.Enumerable;

public struct ChunkEnumerator<T>
{
    private readonly List<T> _list;
    private RangeWorker _rangeWorker;
    private int _fromInclusive;
    private int _toExclusive;

    internal ChunkEnumerator(List<T> list, RangeWorker rangeWorker)
    {
        _list = list;
        _rangeWorker = rangeWorker;
        _fromInclusive = 0;
        _toExclusive = 0;
    }

    public SyncTask<bool> MoveNextAsync()
    {
        if (_fromInclusive++ >= _toExclusive - 1)
        {
            return new SyncTask<bool>(_rangeWorker.FindNewWork(out _fromInclusive, out _toExclusive));
        }

        return new SyncTask<bool>(true);
    }

    public T Current => _list[_fromInclusive];

    [UsedImplicitly] //TODO: R# bug?
    public JoiningTask DisposeAsync()
    {
        return new JoiningTask();
    }
}