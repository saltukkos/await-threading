// MIT License
// Copyright (c) 2023 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

namespace AwaitThreading.Core.Tasks;

public static class ParallelTaskExtensions
{
    public static async Task AsTask(this ParallelTask parallelTask)
    {
        await parallelTask;
    }

    public static async Task<T> AsTask<T>(this ParallelTask<T> parallelTask)
    {
        return await parallelTask;
    }

    public static ValueTask<T> AsValueTask<T>(this ParallelValueTask<T> parallelValueTask)
    {
        return parallelValueTask.Implementation is { } implementation 
            ? new ValueTask<T>(new ParallelTask<T>(implementation).AsTask()) 
            : ValueTask.FromResult(parallelValueTask.Result!);
    }
}