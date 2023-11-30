// MIT License
// Copyright (c) 2023 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Runtime.InteropServices;

namespace AwaitThreading.Core;

[StructLayout(LayoutKind.Sequential, Size = 1)]
internal struct Unit
{
}

public static class Logger
{
    private static readonly Stopwatch Sw = Stopwatch.StartNew();

    [Conditional("DEBUG")]
    public static void Log(string message)
    {
        Console.Out.WriteLine($"{Sw.ElapsedMilliseconds:00000} [id={Thread.CurrentThread.ManagedThreadId}, context={ParallelContext.GetCurrentContexts()}]: {message}");
    }
}