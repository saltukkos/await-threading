// MIT License
// Copyright (c) 2024 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace AwaitThreading.Core;

/// <summary>
/// Represents ambient data that is local to a given forked thread. 
/// </summary>
/// <remarks>
/// Usage flow: ParallelLocal needs to be created before forking,
/// then, forking should be performed using the <see cref="InitializeAndFork"/> method.
/// After that, each thread can get and set it's local value
/// </remarks>
public sealed class ParallelLocal<T> //TODO: it can be a mutable struct, can't it?
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

            var id = ParallelContext.Id;
            return ref _slots[id];
        }
    }
}