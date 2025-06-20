// MIT License
// Copyright (c) 2024 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;

namespace AwaitThreading.Core.Tasks;


[AsyncMethodBuilder(typeof(ParallelValueTaskMethodBuilder))]
public readonly struct ParallelValueTask
{
  public static ParallelValueTask<T> FromResult<T>(T result) => new(result);
  
  internal readonly ParallelTaskImpl<Unit>? Implementation;

  public ParallelValueTask()
  {
      Implementation = null;
  }

  internal ParallelValueTask(ParallelTaskImpl<Unit> parallelTaskImpl)
  {
      Implementation = parallelTaskImpl;
  }
    
  public ParallelValueTaskAwaiter GetAwaiter() => new(this);

}