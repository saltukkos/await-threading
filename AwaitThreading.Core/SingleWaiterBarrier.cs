// MIT License
// Copyright (c) 2023 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

namespace AwaitThreading.Core;

public sealed class SingleWaiterBarrier
{
    private int _count; 

    public SingleWaiterBarrier(int count)
    {
        _count = count;
    }

    public bool Finish()
    {
        return Interlocked.Decrement(ref _count) == 0;
    }
    
    public void Signal()
    {
        lock (this)
        {
            _count--;
            if (_count < 0)
            {
                throw new InvalidOperationException("Too many threads signaled");
            }

            if (_count == 0)
            {
                Monitor.Pulse(this);
            }
        }
    }
    
    public void SignalAndWait()
    {
        lock (this)
        {
            _count--;
            if (_count < 0)
            {
                throw new InvalidOperationException("Too many threads signaled");
            }

            while (_count != 0)
            {
                Monitor.Wait(this);
            }
        }
    }
}