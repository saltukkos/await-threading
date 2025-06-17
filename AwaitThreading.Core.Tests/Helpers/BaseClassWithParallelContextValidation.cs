// MIT License
// Copyright (c) 2024 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace AwaitThreading.Core.Tests.Helpers;

public class BaseClassWithParallelContextValidation
{
    [TearDown]
    public void TearDown()
    {
        var threadsWithParallelContext = new ConcurrentBag<int>();
        var tasksCount = ThreadPool.ThreadCount;

        var tasks = new Task[tasksCount];
        for (var i = 0; i < tasksCount; i++)
        {
            tasks[i] = Task.Run(
                () =>
                {
                    var lastContext = ParallelContext.CaptureAndClear();
                    if (!lastContext.IsEmpty)
                    {
                        Logger.Log($"Non empty context was detected: {lastContext.StackToString()}");
                        threadsWithParallelContext.Add(Thread.CurrentThread.ManagedThreadId);
                    }
                    

                    Thread.Sleep(10);
                });
        }

        Task.WaitAll(tasks);
        Assert.That(threadsWithParallelContext, Is.Empty);
    }

    [DoesNotReturn]
    protected void FailFast([CallerLineNumber] int lineNumber = 0)
    {
        Logger.Log($"Failing at line number {lineNumber}");
        Environment.Exit(lineNumber);
    }
}