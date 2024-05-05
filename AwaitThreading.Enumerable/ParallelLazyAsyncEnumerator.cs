//MIT License
//Copyright (c) 2023 Saltuk Konstantin
//See the LICENSE file in the project root for more information.

using AwaitThreading.Core;
using JetBrains.Annotations;

namespace AwaitThreading.Enumerable;

public readonly struct ParallelLazyAsyncEnumerator<T>
{
    private readonly List<T> _list;
    private readonly int _threadsCount;
    
    // In ideal world we would be able to store enumerator for our chunk in struct field,
    // but any changes to the state of this struct will be lost since async methods are
    // executed on the copy of a struct, so we have to store the data somewhere else.
    private readonly AsyncLocal<IEnumerator<T>> _chunkEnumerator = new ();

    public ParallelLazyAsyncEnumerator(List<T> list, int threadsCount)
    {
        _threadsCount = threadsCount;
        _list = list;
    }

    public async ParallelTask<bool> MoveNextAsync()
    {
        if (_chunkEnumerator.Value is { } chunkEnumerator)
        {
            //TODO: allocation every time is expensive, optimize fastpath. Introduce smth like valueTask
            return chunkEnumerator.MoveNext();
        }

        await new ForkingTask(_threadsCount);
        var context = ParallelContext.GetCurrentFrame();
        var id = context.Id;
        var chunkSize = (_list.Count + _threadsCount - 1) / _threadsCount;
        var start = chunkSize * id;
        var end = chunkSize * (id + 1);
        if (end > _list.Count)
        {
            end = _list.Count;
        }

        // ReSharper disable once NotDisposedResource TODO: can't we track it?
        var enumerator = _list.Skip(start).Take(end - start).GetEnumerator();
        _chunkEnumerator.Value = enumerator;
        
        return enumerator.MoveNext();
    }

    public T Current
    {
        get
        {
            var enumerator = _chunkEnumerator.Value;
            if (enumerator is null)
            {
                return default!;
            }

            return enumerator.Current;
        }
    }

    [UsedImplicitly] //TODO: R# bug?
    public async ParallelTask DisposeAsync()
    {
        _chunkEnumerator.Value?.Dispose();
        await new JoiningTask();
    }
}