// MIT License
// Copyright (c) 2024 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

namespace AwaitThreading.Core.Tests.Helpers;

public sealed class ParallelCounter
{
    private int _count;

    public int Count => _count;

    public void Increment()
    {
        Interlocked.Increment(ref _count);
    }

    public void Add(int value)
    {
        Interlocked.Add(ref _count, value);
    }

    public void AssertCount(int expectedCount)
    {
        Assert.That(_count, Is.EqualTo(expectedCount));
    }
}