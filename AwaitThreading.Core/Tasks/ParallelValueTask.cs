// MIT License
// Copyright (c) 2024 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

namespace AwaitThreading.Core.Tasks;


//TODO: do we need functional ParallelValueTask? E.g. for method like 'ForkIfNeed()' 
public static class ParallelValueTask
{
  public static ParallelValueTask<T> FromResult<T>(T result) => new(result);
}