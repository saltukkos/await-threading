// MIT License
// Copyright (c) 2025 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

namespace AwaitThreading.Core.Context;

public readonly struct ParallelFrame : IEquatable<ParallelFrame>
{
    public readonly int Id;
    public readonly int Count;
    internal readonly SingleWaiterBarrier JoinBarrier;

#if DEBUG
    public readonly string CreationStackTrace;
#endif

    internal ParallelFrame(int id, int count, SingleWaiterBarrier joinBarrier)
    {
        Id = id;
        Count = count;
        JoinBarrier = joinBarrier;
#if DEBUG
        CreationStackTrace = Environment.StackTrace;
#endif
    }

    public object ForkIdentity => JoinBarrier;

    public bool Equals(ParallelFrame other)
    {
        return Id == other.Id && JoinBarrier.Equals(other.JoinBarrier);
    }

    public override bool Equals(object? obj)
    {
        return obj is ParallelFrame other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, JoinBarrier);
    }
}