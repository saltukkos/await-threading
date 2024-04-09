// MIT License
// Copyright (c) 2023 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

namespace AwaitThreading.Core;

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
}