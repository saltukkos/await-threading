// MIT License
// Copyright (c) 2024 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

namespace AwaitThreading.Core;

internal readonly struct BlockingQueue<T>
{
    //TODO: optimize for 1 element?
    private readonly Queue<T> _queue = new();

    public BlockingQueue()
    {
    }

    public int Count => _queue.Count;

    public void Add(T item)
    {
        lock (_queue)
        {
            _queue.Enqueue(item);
            Monitor.Pulse(_queue);
        }
    }

    public T Take()
    {
        lock (_queue)
        {
            for (;;)
            {
                if (_queue.TryDequeue(out var item))
                {
                    return item;
                }

                Monitor.Wait(_queue);
            }
        }
    }
}