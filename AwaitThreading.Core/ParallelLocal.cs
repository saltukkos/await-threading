// MIT License
// Copyright (c) 2024 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

using AwaitThreading.Core.Context;
using JetBrains.Annotations;

namespace AwaitThreading.Core;

/// <summary>
/// Represents ambient data that is local to a given forked thread. 
/// </summary>
/// <remarks>
/// Usage flow: ParallelLocal needs to be created before forking,
/// then, forking should be performed using the <see cref="InitializeAndFork"/> method.
/// After that, each thread can get and set its local value
/// </remarks>
public class ParallelLocal<T>
{
    private T?[]? _slots;

    [MustUseReturnValue]
    public ForkingTask InitializeAndFork(int threadsCount)
    {
        if (_slots is not null)
            Assertion.ThrowInvalidParallelLocalUsage();

        _slots = new T[threadsCount];
        return new ForkingTask(threadsCount);
    }

    public bool IsInitialized => _slots is not null;

    public ref T? Value
    {
        get
        {
            if (_slots is null)
                Assertion.ThrowInvalidParallelLocalUsage();

            var id = ParallelContextStorage.GetTopFrameId();
            return ref _slots[id];
        }
    }
}