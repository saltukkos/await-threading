// MIT License
// Copyright (c) 2024 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

using AwaitThreading.Core;
using JetBrains.Annotations;

namespace AwaitThreading.Enumerable;

public interface IParallelAsyncEnumerator<out T>
{
    SyncTask<bool> MoveNextAsync();

    T Current { get; }

    // TODO: ParallelTask would be more universal
    [UsedImplicitly]
    JoiningTask DisposeAsync();
}