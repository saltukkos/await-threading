// MIT License
// Copyright (c) 2023 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.ExceptionServices;

namespace AwaitThreading.Core;

internal static class Assertion
{
    private const string? BadAwaitMessage = "Regular async methods do not support forking. Use ParallelTask as a method's return value.";

    public static readonly ExceptionDispatchInfo BadAwaitExceptionDispatchInfo =
        ExceptionDispatchInfo.Capture(new InvalidOperationException(BadAwaitMessage));

    [DoesNotReturn]
    public static void ThrowBadAwait() => throw new InvalidOperationException(BadAwaitMessage);

    [DoesNotReturn]
    public static void ThrowInvalidTasksCount() => throw new InvalidOperationException("Fork should have positive number of threads.");

    [DoesNotReturn]
    public static void ThrowInvalidDirectGetResultCall() => throw new NotSupportedException($"Do not call .GetResult() directly on ParallelTask, use .{nameof(ParallelTaskExtensions.AsTask)}().Wait()");

    [DoesNotReturn]
    public static void ThrowInvalidParallelLocalUsage() => throw new InvalidOperationException("ParallelLocal should be initialized while forking");
}