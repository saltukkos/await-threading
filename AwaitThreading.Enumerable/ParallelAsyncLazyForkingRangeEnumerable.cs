//MIT License
//Copyright (c) 2023 Saltuk Konstantin
//See the LICENSE file in the project root for more information.

using AwaitThreading.Core.Operations;

namespace AwaitThreading.Enumerable;

public sealed class ParallelAsyncLazyForkingRangeEnumerable<T> : IParallelAsyncLazyForkingEnumerable<T>
{
    private readonly IReadOnlyList<T> _list;
    private readonly int _threadsCount;
    private readonly ForkingOptions? _forkingOptions;

    public ParallelAsyncLazyForkingRangeEnumerable(IReadOnlyList<T> list, int threadsCount, ForkingOptions? forkingOptions)
    {
        _list = list;
        _threadsCount = threadsCount;
        _forkingOptions = forkingOptions;
    }

    IParallelAsyncLazyForkingEnumerator<T> IParallelAsyncLazyForkingEnumerable<T>.GetAsyncEnumerator()
    {
        return GetAsyncEnumerator();
    }

    public ParallelAsyncLazyForkingRangeEnumerator<T> GetAsyncEnumerator()
    {
        return new ParallelAsyncLazyForkingRangeEnumerator<T>(_list, _threadsCount, _forkingOptions);
    }
}