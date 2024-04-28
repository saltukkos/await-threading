//MIT License
//Copyright (c) 2023 Saltuk Konstantin
//See the LICENSE file in the project root for more information.

using AwaitThreading.Core;
using JetBrains.Annotations;

namespace AwaitThreading.Enumerable;

public struct ParallelAsyncEnumerator<T>
{
    private readonly List<T> _list;
    private readonly int _maxIndex;
    private int _currentIndex;

    public ParallelAsyncEnumerator(List<T> list, int startIndex, int endIndex)
    {
        _list = list;
        _maxIndex = endIndex - 1;
        _currentIndex = startIndex;
    }

    public SyncTask<bool> MoveNextAsync()
    {
        if (_currentIndex >= _maxIndex)
            return new SyncTask<bool>(false);

        _currentIndex++;
        return new SyncTask<bool>(true);
    }

    public T Current => _list[_currentIndex];

    [UsedImplicitly] //TODO: R# bug?
    public JoiningTask DisposeAsync()
    {
        return new JoiningTask();
    }
}