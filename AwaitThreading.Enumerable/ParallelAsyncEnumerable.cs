//MIT License
//Copyright (c) 2023 Saltuk Konstantin
//See the LICENSE file in the project root for more information.

namespace AwaitThreading.Enumerable;

public readonly struct ChunkEnumerable<T>
{
    private readonly List<T> _list;
    private readonly int _start;
    private readonly int _end;

    public ChunkEnumerable(List<T> list, int start, int end)
    {
        _list = list;
        _start = start;
        _end = end;
    }
    
    public ChunkEnumerator<T> GetAsyncEnumerator()
    {
        return new ChunkEnumerator<T>(_list, _start, _end);
    }
}