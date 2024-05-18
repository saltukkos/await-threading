// MIT License
// Copyright (c) 2024 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.ExceptionServices;

namespace AwaitThreading.Core;

public readonly struct ParallelTaskResult<T>
{
    public ParallelTaskResult(T result)
    {
        Result = result;
        ExceptionDispatchInfo = null;
    }

    public ParallelTaskResult(ExceptionDispatchInfo exceptionDispatchInfo)
    {
        ExceptionDispatchInfo = exceptionDispatchInfo;
        Result = default;
    }

    [MemberNotNullWhen(true, nameof(Result))]
    [MemberNotNullWhen(false, nameof(ExceptionDispatchInfo))]
    public bool HasResult => ExceptionDispatchInfo is null;

    public readonly T? Result;

    public readonly ExceptionDispatchInfo? ExceptionDispatchInfo;
}