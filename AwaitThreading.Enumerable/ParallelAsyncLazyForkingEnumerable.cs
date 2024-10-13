//MIT License
//Copyright (c) 2023 Saltuk Konstantin
//See the LICENSE file in the project root for more information.

namespace AwaitThreading.Enumerable;

public sealed class ParallelAsyncLazyForkingEnumerable<T>
{
    private readonly IReadOnlyList<T> _list;
    private readonly int _threadsCount;

    public ParallelAsyncLazyForkingEnumerable(IReadOnlyList<T> list, int threadsCount)
    {
        _list = list;
        _threadsCount = threadsCount;
    }
    
    public ParallelAsyncLazyForkingEnumerator<T> GetAsyncEnumerator()
    {
        return new ParallelAsyncLazyForkingEnumerator<T>(_list, _threadsCount);
    }
}