// MIT License
// Copyright (c) 2023 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

using AwaitThreading.Core.Diagnostics;

namespace AwaitThreading.Core.Context;

internal sealed class SingleWaiterBarrier
{
    internal int Count; 

    public SingleWaiterBarrier(int count)
    {
        Count = count;
    }

    public bool Finish()
    {
        var decrementedValue = Interlocked.Decrement(ref Count);
        if (decrementedValue < 0)
        {
            Assertion.StateCorrupted("Too many threads signaled");
        }

        return decrementedValue == 0;
    }
    
    public void Signal()
    {
        lock (this)
        {
            Count--;
            if (Count < 0)
            {
                Assertion.StateCorrupted("Too many threads signaled");
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
                Assertion.StateCorrupted("Too many threads signaled");
            }

            while (Count != 0)
            {
                Monitor.Wait(this);
            }
        }
    }
}