// MIT License
// Copyright (c) 2024 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

namespace AwaitThreading.Enumerable.Experimental;

public static class ListExperimentalExtensions
{
  //[Experimental]
  public static ParallelLazyAsyncEnumerator<T> GetAsyncEnumerator<T>(this List<T> list)
  {
    return new ParallelLazyAsyncEnumerator<T>(list, Environment.ProcessorCount);
  }

}