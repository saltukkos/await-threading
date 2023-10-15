//MIT License
//Copyright (c) 2023 Saltuk Konstantin
//See the LICENSE file in the project root for more information.

using AwaitThreading.Core;
using AwaitThreading.Enumerable;

namespace Samples;

public class Program
{
    public static void Main(string[] args)
    {
        Console.Out.WriteLine(JustGiveMeAValueAsyncWrapper().GetResult());
        
        Foreach3().GetResult();
        Foreach3().GetResult();

//        ForkAndJoin().GetResult();
        ForkAndJoinWithAwaits().GetResult();
        Foreach().GetResult();
        Foreach2().GetResult();
        Console.Out.WriteLine(
            $"Finish thread={Thread.CurrentThread.ManagedThreadId} stack: {ParallelContext.GetCurrentContexts()}");
    }

    private static async ParallelTask<int> JustGiveMeAValueAsyncWrapper()
    {
        return await JustGiveMeAValueAsync();
    }

    
    private static async ParallelTask<int> JustGiveMeAValueAsync()
    {
        return 42;
    }
    
    private static async ParallelTask ForkAndJoin()
    {
        await new ForkingTask(2);
        await new JoiningTask();
        
        var random = new Random(Seed: 42);
        Console.Out.WriteLine($"Before fork: thread={Thread.CurrentThread.ManagedThreadId}, stack: {ParallelContext.GetCurrentContexts()}");
        await new ForkingTask(2);
        Thread.Sleep(random.Next() % 100);
        Console.Out.WriteLine($"After fork: thread={Thread.CurrentThread.ManagedThreadId}, stack: {ParallelContext.GetCurrentContexts()}");
        await ForkTwice();
        Thread.Sleep(random.Next() % 100);
        Console.Out.WriteLine($"After fork twice: thread={Thread.CurrentThread.ManagedThreadId}, stack: {ParallelContext.GetCurrentContexts()}");
        await JoinTwice();
        Thread.Sleep(random.Next() % 100);
        Console.Out.WriteLine($"After join twice: thread={Thread.CurrentThread.ManagedThreadId}, stack: {ParallelContext.GetCurrentContexts()}");
        await new JoiningTask();
        Console.Out.WriteLine($"After last join: thread={Thread.CurrentThread.ManagedThreadId}, stack: {ParallelContext.GetCurrentContexts()}");
    }

    private static async ParallelTask ForkAndJoinWithAwaits()
    {
        var random = new Random(Seed: 4242);
        Console.Out.WriteLine($"Before fork: thread={Thread.CurrentThread.ManagedThreadId}, stack: {ParallelContext.GetCurrentContexts()}");
        await new ForkingTask(2);

        Console.Out.WriteLine($"After fork (before delay): thread={Thread.CurrentThread.ManagedThreadId}, stack: {ParallelContext.GetCurrentContexts()}");
        await Task.Delay(random.Next() % 100);
        Console.Out.WriteLine($"After fork (after delay): thread={Thread.CurrentThread.ManagedThreadId}, stack: {ParallelContext.GetCurrentContexts()}");
        
        await ForkTwice();
        
        Console.Out.WriteLine($"After fork twice (before delay): thread={Thread.CurrentThread.ManagedThreadId}, stack: {ParallelContext.GetCurrentContexts()}");
        await Task.Delay(random.Next() % 100);
        Console.Out.WriteLine($"After fork twice (after delay): thread={Thread.CurrentThread.ManagedThreadId}, stack: {ParallelContext.GetCurrentContexts()}");
        
        await JoinTwice();
        await Task.Delay(random.Next() % 100);
        Console.Out.WriteLine($"After join twice: thread={Thread.CurrentThread.ManagedThreadId}, stack: {ParallelContext.GetCurrentContexts()}");
        await new JoiningTask();
        Console.Out.WriteLine($"After last join: thread={Thread.CurrentThread.ManagedThreadId}, stack: {ParallelContext.GetCurrentContexts()}");
    }
    
    private static async ParallelTask ForkTwice()
    {
        await new ForkingTask(2);
        await new ForkingTask(3);
    }

    private static async ParallelTask JoinTwice()
    {
        await new JoiningTask();
        await new JoiningTask();
    }

    private static async ParallelTask Foreach()
    {
        var a = new List<int> {1, 2, 3, 4, 5, 6, 7, 8, 9};
        
        Console.Out.WriteLine($"Before foreach 1: thread={Thread.CurrentThread.ManagedThreadId} stack: {ParallelContext.GetCurrentContexts()}");
        
        await foreach (var n in await a.AsParallelAsync(3))
        {
            Console.Out.WriteLine($"Inside foreach 1: thread={Thread.CurrentThread.ManagedThreadId}, value={n} stack: {ParallelContext.GetCurrentContexts()}");
            Thread.Sleep(1);
        }
        
        Console.Out.WriteLine($"After foreach 1: thread={Thread.CurrentThread.ManagedThreadId} stack: {ParallelContext.GetCurrentContexts()}");
    }

    private static async ParallelTask Foreach2()
    {
        var a = new List<int> {1, 2, 3, 4, 5, 6, 7, 8, 9};
        
        Console.Out.WriteLine($"Before foreach 2: thread={Thread.CurrentThread.ManagedThreadId}");
        
        await foreach (var n in a.AsParallel(3))
        {
            Console.Out.WriteLine($"Inside foreach 2: thread={Thread.CurrentThread.ManagedThreadId}, value={n}");
            Thread.Sleep(1);
        }
        
        Console.Out.WriteLine($"After foreach 2: thread={Thread.CurrentThread.ManagedThreadId}");
    }

    private static async ParallelTask Foreach3()
    {
        var a = new List<int> {1, 2, 3, 4, 5, 6, 7, 8, 9};
        
        Console.Out.WriteLine($"Before foreach 3: thread={Thread.CurrentThread.ManagedThreadId}");
        
        await foreach (var n in a.AsParallel(3))
        {
            Console.Out.WriteLine($"Inside foreach 3: thread={Thread.CurrentThread.ManagedThreadId}, value={n}");
            //await Task.Delay(1).ConfigureAwait(false);
        }
        
        Console.Out.WriteLine($"After foreach 3 s: thread={Thread.CurrentThread.ManagedThreadId}");
    }
}