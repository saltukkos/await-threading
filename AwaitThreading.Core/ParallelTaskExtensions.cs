// MIT License
// Copyright (c) 2023 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

namespace AwaitThreading.Core;

public static class ParallelTaskExtensions
{
    public static Task WaitAsync(this ParallelTask parallelTask)
    {
        var taskCompletionSource = new TaskCompletionSource();
        // TODO: validate there is no forking\joining inside parallelTask
        parallelTask.GetAwaiter().ParallelOnCompleted(() =>
        {
            parallelTask.GetResult();
            taskCompletionSource.SetResult();
        });

        return taskCompletionSource.Task;
    }

    public static Task<T> WaitAsync<T>(this ParallelTask<T> parallelTask)
    {
        var taskCompletionSource = new TaskCompletionSource<T>();
        // TODO: validate there is no forking\joining inside parallelTask
        parallelTask.GetAwaiter().ParallelOnCompleted(() =>
        {
            taskCompletionSource.SetResult(parallelTask.GetResult());
        });

        return taskCompletionSource.Task;
    }
}