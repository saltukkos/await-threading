// MIT License
// Copyright (c) 2024 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

namespace AwaitThreading.Enumerable.Experimental;

public static class ListExperimentalExtensions
{
  //[Experimental]
  public static ParallelAsyncLazyForkingEnumerator<T> GetAsyncEnumerator<T>(this T[] list)
  {
    return new ParallelAsyncLazyForkingEnumerator<T>(list, Environment.ProcessorCount);
  }

}