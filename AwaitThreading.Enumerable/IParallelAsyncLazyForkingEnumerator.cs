// MIT License
// Copyright (c) 2024 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

using AwaitThreading.Core;
using JetBrains.Annotations;

namespace AwaitThreading.Enumerable;

public interface IParallelAsyncLazyForkingEnumerator<T>
{
    ParallelValueTask<bool> MoveNextAsync();
    T Current { get; }

    //TODO: detect in usage analysis
    [UsedImplicitly]
    ParallelTask DisposeAsync();
}