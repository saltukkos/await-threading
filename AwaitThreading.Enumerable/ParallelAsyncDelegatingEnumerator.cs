// MIT License
// Copyright (c) 2024 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

using AwaitThreading.Core;

namespace AwaitThreading.Enumerable;

public class ParallelAsyncDelegatingEnumerator<T> : IParallelAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _enumerator;

    internal ParallelAsyncDelegatingEnumerator(IEnumerator<T> enumerator)
    {
        _enumerator = enumerator;
    }

    public SyncTask<bool> MoveNextAsync()
    {
        return new SyncTask<bool>(_enumerator.MoveNext());
    }

    public T Current => _enumerator.Current;

    public JoiningTask DisposeAsync()
    {
        _enumerator.Dispose();
        return new JoiningTask();
    }
}