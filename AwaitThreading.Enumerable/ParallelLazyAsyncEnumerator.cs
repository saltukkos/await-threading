//MIT License
//Copyright (c) 2023 Saltuk Konstantin
//See the LICENSE file in the project root for more information.

using AwaitThreading.Core;

namespace AwaitThreading.Enumerable;

public readonly struct ParallelLazyAsyncEnumerator<T>
{
    private readonly List<T> _list;
    private readonly int _threadsCount;
    private readonly ThreadLocal<IEnumerator<T>> _threadLocal = new ();

    public ParallelLazyAsyncEnumerator(List<T> list, int threadsCount)
    {
        _threadsCount = threadsCount;
        _list = list;
    }

    public async ParallelTask<bool> MoveNextAsync()
    {
        if (_threadLocal.IsValueCreated)
        {
            return _threadLocal.Value!.MoveNext();
        }

        await new ForkingTask(_threadsCount);
        var context = ParallelContext.GetCurrentContext();
        var id = context!.Value.Id;
        var chunkSize = (_list.Count + _threadsCount - 1) / _threadsCount;
        var start = chunkSize * id;
        var end = chunkSize * (id + 1);
        if (end > _list.Count)
        {
            end = _list.Count;
        }

        var enumerator = _list.Skip(start).Take(end - start).GetEnumerator();
        _threadLocal.Value = enumerator;
        return enumerator.MoveNext();
    }

    public T Current
    {
        get
        {
            var enumerator = _threadLocal.Value;
            if (enumerator is null)
            {
                return default!;
            }

            return enumerator.Current;
        }
    }

    public JoiningTask DisposeAsync()
    {
//        _threadLocal.Dispose(); TODO implement
        return new JoiningTask();
    }
}