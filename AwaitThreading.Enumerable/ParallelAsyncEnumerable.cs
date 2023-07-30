//MIT License
//Copyright (c) 2023 Saltuk Konstantin
//See the LICENSE file in the project root for more information.

namespace AwaitThreading.Enumerable;

public sealed class ParallelAsyncEnumerable<T>
{
    private readonly List<T> _subList;

    public ParallelAsyncEnumerable(List<T> subList)
    {
        _subList = subList;
    }
    
    public ParallelAsyncEnumerator<T> GetAsyncEnumerator()
    {
        return new ParallelAsyncEnumerator<T>(_subList.GetEnumerator());
    }
}