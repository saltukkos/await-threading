// MIT License
// Copyright (c) 2024 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

namespace AwaitThreading.Core;

public static class ParallelValueTask
{
  public static ParallelValueTask<T> FromResult<T>(T result) => new(result);
}