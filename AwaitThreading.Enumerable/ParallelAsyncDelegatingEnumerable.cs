// MIT License
// Copyright (c) 2024 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

namespace AwaitThreading.Enumerable;

public class ParallelAsyncDelegatingEnumerable<T> : IParallelAsyncEnumerable<T>
{
    private readonly IEnumerator<T> _enumerator;

    public ParallelAsyncDelegatingEnumerable(IEnumerator<T> enumerator)
    {
        _enumerator = enumerator;
    }

    IParallelAsyncEnumerator<T> IParallelAsyncEnumerable<T>.GetAsyncEnumerator()
    {
        return GetAsyncEnumerator();
    }

    public ParallelAsyncDelegatingEnumerator<T> GetAsyncEnumerator()
    {
        return new ParallelAsyncDelegatingEnumerator<T>(_enumerator);
    }
}