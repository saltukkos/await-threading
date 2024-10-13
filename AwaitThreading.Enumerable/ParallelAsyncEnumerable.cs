//MIT License
//Copyright (c) 2023 Saltuk Konstantin
//See the LICENSE file in the project root for more information.

namespace AwaitThreading.Enumerable;

public readonly struct ParallelAsyncEnumerable<T>
{
    private readonly IReadOnlyList<T> _list;
    private readonly RangeWorker _rangeWorker;

    internal ParallelAsyncEnumerable(IReadOnlyList<T> list, RangeWorker rangeWorker)
    {
        _list = list;
        _rangeWorker = rangeWorker;
    }
    
    public ParallelAsyncEnumerator<T> GetAsyncEnumerator()
    {
        return new ParallelAsyncEnumerator<T>(_list, _rangeWorker);
    }
}