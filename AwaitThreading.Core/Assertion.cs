// MIT License
// Copyright (c) 2023 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

using System.Diagnostics.CodeAnalysis;

namespace AwaitThreading.Core;

internal static class Assertion
{
    [DoesNotReturn]
    public static void ThrowInvalidTaskIsUsed() => throw new NotSupportedException("Only ParallelTask methods are supported");
}