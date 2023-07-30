//MIT License
//Copyright (c) 2023 Saltuk Konstantin
//See the LICENSE file in the project root for more information.

namespace AwaitThreading.Enumerable;

public sealed class ParallelLazyAsyncEnumerable<T>
{
    private readonly List<T> _list;
    private readonly int _threadsCount;

    public ParallelLazyAsyncEnumerable(List<T> list, int threadsCount)
    {
        _list = list;
        _threadsCount = threadsCount;
    }
    
    public ParallelLazyAsyncEnumerator<T> GetAsyncEnumerator()
    {
        return new ParallelLazyAsyncEnumerator<T>(_list, _threadsCount);
    }
}