// MIT License
// Copyright (c) 2024 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;

namespace AwaitThreading.Core;

[AsyncMethodBuilder(typeof(ParallelValueTaskMethodBuilder<>))]
public readonly struct ParallelValueTask<T>
{
    // Note: we could implement a sync exception workload, but ParallelValueTask is implemented only
    // for performance optimizations and exceptions are not a priority here. 
    internal readonly T? Result;
    internal readonly ParallelTaskImpl<T>? Implementation;

    public ParallelValueTask(T result)
    {
        Result = result;
        Implementation = null;
    }

    internal ParallelValueTask(ParallelTaskImpl<T> parallelTaskImpl)
    {
        Result = default;
        Implementation = parallelTaskImpl;
    }
    
    public ParallelValueTaskAwaiter<T> GetAwaiter() => new(this);
}