// MIT License
// Copyright (c) 2023 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

namespace AwaitThreading.Core;

public static class ParallelTaskExtensions
{
    // TODO: can we do something like it automatically, when normal OnCompleted is called?
    public static Task WaitAsync(this ParallelTask parallelTask)
    {
        var taskCompletionSource = new TaskCompletionSource();
        // TODO: validate there is no forking\joining inside parallelTask
        parallelTask.GetAwaiter().ParallelOnCompleted(() =>
        {
            var exceptionDispatchInfo = parallelTask.GetResult();
            if (exceptionDispatchInfo is null)
            {
                taskCompletionSource.SetResult();
            }
            else
            {
                //TODO: how to join ExceptionDispatchInfo and TaskCompletionSource?
                taskCompletionSource.SetException(exceptionDispatchInfo.SourceException);
            }
        });

        return taskCompletionSource.Task;
    }

    public static Task<T> WaitAsync<T>(this ParallelTask<T> parallelTask)
    {
        var taskCompletionSource = new TaskCompletionSource<T>();
        parallelTask.GetAwaiter().ParallelOnCompleted(() =>
        {
            var result = parallelTask.GetResult();
            if (result.HasResult)
            {
                taskCompletionSource.SetResult(result.Result);
            }
            else
            {
                taskCompletionSource.SetException(result.ExceptionDispatchInfo.SourceException);
            }
        });

        return taskCompletionSource.Task;
    }
}