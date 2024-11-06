// MIT License
// Copyright (c) 2024 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

namespace AwaitThreading.Core.Tests.Helpers;

public class BaseClassWithParallelContextValidation
{
    [TearDown]
    public void TearDown()
    {
        var hasThreadsWithParallelContext = false;
        var tasksCount = ThreadPool.ThreadCount;

        var tasks = new Task[tasksCount];
        for (var i = 0; i < tasksCount; i++)
        {
            tasks[i] = Task.Run(
                () =>
                {
                    if (ParallelContext.GetCurrentFrameSafe() != null)
                        hasThreadsWithParallelContext = true;

                    Thread.Sleep(10);
                });
        }

        Task.WaitAll(tasks);
        Assert.That(hasThreadsWithParallelContext, Is.False);
    }
}