// MIT License
// Copyright (c) 2025 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace AwaitThreading.Core.Context;

public static class ParallelContextStorage
{
    [field: ThreadStatic]
    public static ParallelContext CurrentThreadContext { get; internal set; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetTopFrameId() => CurrentThreadContext.GetTopFrame().Id;

    internal static ParallelFrame PopFrame()
    {
        var parallelContext = CurrentThreadContext.PopFrame(out var poppedFrame);
        CurrentThreadContext = parallelContext;
        return poppedFrame;
    }

    internal static ParallelContext CaptureAndClear()
    {
        var currentContext = CurrentThreadContext;
        CurrentThreadContext = default;
        Logger.Log("Context cleared");
        return currentContext;
    }

    internal static void ClearButNotExpected()
    {
        VerifyContextIsEmpty();
        CurrentThreadContext = default;
    }

    internal static void Restore(ParallelContext context)
    {
        VerifyContextIsEmpty();
        CurrentThreadContext = context;
        Logger.Log("Context restored");
    }

    [Conditional("DEBUG")]
    private static void VerifyContextIsEmpty()
    {
        if (!CurrentThreadContext.IsEmpty)
        {
            Logger.Log("Context is not empty when expected to be");
            Debug.Fail("Context already exists");
        }
    }
}