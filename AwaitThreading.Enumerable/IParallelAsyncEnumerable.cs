// MIT License
// Copyright (c) 2024 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

namespace AwaitThreading.Enumerable;

public interface IParallelAsyncEnumerable<out T>
{
    IParallelAsyncEnumerator<T> GetAsyncEnumerator();
}