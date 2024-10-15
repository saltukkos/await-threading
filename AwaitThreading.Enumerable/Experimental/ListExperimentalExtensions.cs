// MIT License
// Copyright (c) 2024 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

namespace AwaitThreading.Enumerable.Experimental;

public static class ListExperimentalExtensions
{
  //[Experimental]
  public static ParallelAsyncLazyForkingRangeEnumerator<T> GetAsyncEnumerator<T>(this IReadOnlyList<T> list)
  {
    return new ParallelAsyncLazyForkingRangeEnumerator<T>(list, Environment.ProcessorCount);
  }

}