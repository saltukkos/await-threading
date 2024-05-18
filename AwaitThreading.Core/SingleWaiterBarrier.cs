// MIT License
// Copyright (c) 2023 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

namespace AwaitThreading.Core;

public sealed class SingleWaiterBarrier
{
    internal int Count; 

    public SingleWaiterBarrier(int count)
    {
        Count = count;
    }

    public bool Finish()
    {
        return Interlocked.Decrement(ref Count) == 0;
    }
    
    public void Signal()
    {
        lock (this)
        {
            Count--;
            if (Count < 0)
            {
                throw new InvalidOperationException("Too many threads signaled");
            }

            if (Count == 0)
            {
                Monitor.Pulse(this);
            }
        }
    }
    
    public void SignalAndWait()
    {
        lock (this)
        {
            Count--;
            if (Count < 0)
            {
                throw new InvalidOperationException("Too many threads signaled");
            }

            while (Count != 0)
            {
                Monitor.Wait(this);
            }
        }
    }
}