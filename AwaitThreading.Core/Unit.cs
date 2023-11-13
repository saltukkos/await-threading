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

public static class Tim
{
    private static Stopwatch _sw = Stopwatch.StartNew();
    public static string Er => $"{_sw.ElapsedMilliseconds:00000}: ";
}