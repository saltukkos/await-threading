// MIT License
// Copyright (c) 2024 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using AwaitThreading.Core.Context;

namespace AwaitThreading.Core.Diagnostics;

public static class Logger
{
    private static Stopwatch? _stopwatch;

    [Conditional("DEBUG")]
    public static void Log(string message)
    {
        _stopwatch ??= Stopwatch.StartNew();
        Console.Out.WriteLine($"{_stopwatch.ElapsedTicks:0000000000}/{_stopwatch.ElapsedMilliseconds:00000} [id={Thread.CurrentThread.ManagedThreadId}, context={ParallelContextStorage.CurrentThreadContext.StackToString()}]: {message}");
    }
}