// MIT License
// Copyright (c) 2023 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using AwaitThreading.Core.Tasks;

namespace AwaitThreading.Core.Diagnostics;

internal static class Assertion
{
    private const string? BadAwaitMessage = "Regular async methods do not support forking. Use ParallelTask as a method's return value.";

    public static readonly ExceptionDispatchInfo BadAwaitExceptionDispatchInfo =
        ExceptionDispatchInfo.Capture(new InvalidOperationException(BadAwaitMessage));

    [DoesNotReturn]
    public static void StateCorrupted(string message)
    {
        Debug.Fail(message);
        throw new StateCorruptedException(message);
    }

    [DoesNotReturn]
    public static void ThrowBadAwait() => throw new InvalidOperationException(BadAwaitMessage);

    [DoesNotReturn]
    public static void ThrowInvalidTasksCount(int actualCount, [CallerArgumentExpression("actualCount")] string? paramName = null)
    {
        throw new ArgumentOutOfRangeException(paramName, actualCount, "Fork should have positive number of threads");
    }

    // TODO: mark 'GetResult' methods as Obsolete (considering they are not shown in compiler-generated code)?
    [DoesNotReturn]
    public static void ThrowInvalidDirectGetResultCall() => throw new NotSupportedException($"Do not call .GetResult() directly on ParallelTask, use .{nameof(ParallelTaskExtensions.AsTask)}().Wait()");

    [DoesNotReturn]
    public static void ThrowInvalidSecondAwaitOfParallelTask() => throw new InvalidOperationException("Parallel task can't be awaited twice");

    [DoesNotReturn]
    public static void ThrowInvalidParallelLocalUsage() => throw new InvalidOperationException("ParallelLocal should be initialized while forking");

    private class StateCorruptedException : Exception
    {
        public StateCorruptedException(string message) : base(message) { }
    }
}