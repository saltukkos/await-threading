//MIT License
//Copyright (c) 2023 Saltuk Konstantin
//See the LICENSE file in the project root for more information.

using AwaitThreading.Core;

namespace AwaitThreading.Enumerable;

public struct ParallelAsyncEnumerator<T>
{
    private List<T>.Enumerator _enumerator;

    public ParallelAsyncEnumerator(List<T>.Enumerator enumerator)
    {
        _enumerator = enumerator;
    }

    public ValueTask<bool> MoveNextAsync() => ValueTask.FromResult(_enumerator.MoveNext());
    public T Current => _enumerator.Current;
    public JoiningTask DisposeAsync() => new JoiningTask();

}