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

        // ThreadPool.SetMinThreads(5, 100);
        Console.Out.WriteLine($"{Tim.Er} start!");
        MinimalRepro().GetResult();
        // return;
        
        
        // Console.Out.WriteLine($"{Tim.Er}");
        Console.Out.WriteLine(JustGiveMeAValueAsyncWrapper().GetResult());
        //
        Foreach3().GetResult();
        Foreach3().GetResult();
        //
        ForkAndJoin().GetResult();
        ForkAndJoinWithAwaits().GetResult();
        Foreach().GetResult();
        Foreach().GetResult();
        Foreach2().GetResult();
        Console.Out.WriteLine(
            $"{Tim.Er}Finish thread={Thread.CurrentThread.ManagedThreadId} stack: {ParallelContext.GetCurrentContexts()}");
    }

    private static async ParallelTask MinimalRepro()
    {
        // for (int i = 0; i < 1; ++i)
        {
            await new ForkingTask(100);
            Console.Out.WriteLine($"{Tim.Er} Forked! {ParallelContext.GetCurrentContexts()} at thread {Thread.CurrentThread.ManagedThreadId}");
            //await Task.Delay(1);
            // Console.Out.WriteLine($"{Tim.Er} Delayed! {Thread.CurrentThread.ManagedThreadId}");
            await JoinOnce();
            Console.Out.WriteLine($"{Tim.Er} Joined! {Thread.CurrentThread.ManagedThreadId}");
        }

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
        // await new ForkingTask(2);
        // await new JoiningTask();
        
        var random = new Random(Seed: 42);
        
        Console.Out.WriteLine($"{Tim.Er}After fork twice (before delay): thread={Thread.CurrentThread.ManagedThreadId}, stack: {ParallelContext.GetCurrentContexts()}");
        await Task.Yield();
        Console.Out.WriteLine($"{Tim.Er}After fork twice (after delay): thread={Thread.CurrentThread.ManagedThreadId}, stack: {ParallelContext.GetCurrentContexts()}");

        Console.Out.WriteLine($"{Tim.Er}Before fork: thread={Thread.CurrentThread.ManagedThreadId}, stack: {ParallelContext.GetCurrentContexts()}");
        await new ForkingTask(2);
        Thread.Sleep(random.Next() % 10);
        Console.Out.WriteLine($"{Tim.Er}After fork: thread={Thread.CurrentThread.ManagedThreadId}, stack: {ParallelContext.GetCurrentContexts()}");
        await ForkTwice();
        Thread.Sleep(random.Next() % 10);
        Console.Out.WriteLine($"{Tim.Er}After fork twice: thread={Thread.CurrentThread.ManagedThreadId}, stack: {ParallelContext.GetCurrentContexts()}");
        await JoinTwice();
        Thread.Sleep(random.Next() % 10);
        Console.Out.WriteLine($"{Tim.Er}After join twice: thread={Thread.CurrentThread.ManagedThreadId}, stack: {ParallelContext.GetCurrentContexts()}");
        await new JoiningTask();
        Console.Out.WriteLine($"{Tim.Er}After last join: thread={Thread.CurrentThread.ManagedThreadId}, stack: {ParallelContext.GetCurrentContexts()}");
    }

    private static async ParallelTask ForkAndJoinWithAwaits()
    {
        var random = new Random(Seed: 4242);
        Console.Out.WriteLine($"{Tim.Er}Before fork: thread={Thread.CurrentThread.ManagedThreadId}, stack: {ParallelContext.GetCurrentContexts()}");
        await new ForkingTask(2);

        Console.Out.WriteLine($"{Tim.Er}After fork (before delay 1): thread={Thread.CurrentThread.ManagedThreadId}, stack: {ParallelContext.GetCurrentContexts()}");
        // await Task.Delay(random.Next() % 10);
        await Task.Yield();
        Console.Out.WriteLine($"{Tim.Er}After fork (after delay 1): thread={Thread.CurrentThread.ManagedThreadId}, stack: {ParallelContext.GetCurrentContexts()}");
        
        // await ForkTwice();
        await new ForkingTask(2);
        await new ForkingTask(3);
        
        Console.Out.WriteLine($"{Tim.Er}After fork twice (before delay 2): thread={Thread.CurrentThread.ManagedThreadId}, stack: {ParallelContext.GetCurrentContexts()}");
        // await Task.Delay(random.Next() % 10);
        await Task.Yield();
        Console.Out.WriteLine($"{Tim.Er}After fork twice (after delay 2): thread={Thread.CurrentThread.ManagedThreadId}, stack: {ParallelContext.GetCurrentContexts()}");

        await JoinOnce();
        await JoinOnce();
        // await JoinTwice();
        // await new JoiningTask();
        // await new JoiningTask();

        await Task.Delay(random.Next() % 10);
        Console.Out.WriteLine($"{Tim.Er}After join twice: thread={Thread.CurrentThread.ManagedThreadId}, stack: {ParallelContext.GetCurrentContexts()}");
        await new JoiningTask();
        Console.Out.WriteLine($"{Tim.Er}After last join: thread={Thread.CurrentThread.ManagedThreadId}, stack: {ParallelContext.GetCurrentContexts()}");
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

    private static async ParallelTask JoinOnce()
    {
        await new JoiningTask();
    }

    private static async ParallelTask Foreach()
    {
        var a = new List<int> {1, 2, 3, 4, 5, 6, 7, 8, 9};
        
        Console.Out.WriteLine($"{Tim.Er}Before foreach 1: thread={Thread.CurrentThread.ManagedThreadId} stack: {ParallelContext.GetCurrentContexts()}");
        
        await foreach (var n in await a.AsParallelAsync(3))
        {
            Console.Out.WriteLine($"{Tim.Er}" +
                                  $"Inside foreach 1: thread={Thread.CurrentThread.ManagedThreadId}, value={n} stack: {ParallelContext.GetCurrentContexts()}");
            Thread.Sleep(1);
        }
        
        Console.Out.WriteLine($"{Tim.Er}After foreach 1: thread={Thread.CurrentThread.ManagedThreadId} stack: {ParallelContext.GetCurrentContexts()}");
    }

    private static async ParallelTask Foreach2()
    {
        var a = new List<int> {1, 2, 3, 4, 5, 6, 7, 8, 9};
        
        Console.Out.WriteLine($"{Tim.Er}Before foreach 2: thread={Thread.CurrentThread.ManagedThreadId}");
        
        await foreach (var n in a.AsParallel(3))
        {
            Console.Out.WriteLine($"{Tim.Er}Inside foreach 2: thread={Thread.CurrentThread.ManagedThreadId}, value={n}");
            Thread.Sleep(1);
        }
        
        Console.Out.WriteLine($"{Tim.Er}After foreach 2: thread={Thread.CurrentThread.ManagedThreadId}");
    }

    private static async ParallelTask Foreach3()
    {
        var a = new List<int> {1, 2, 3, 4, 5, 6, 7, 8, 9};
        
        Console.Out.WriteLine($"{Tim.Er}Before foreach 3: thread={Thread.CurrentThread.ManagedThreadId}");
        
        await foreach (var n in a.AsParallel(3))
        {
            Console.Out.WriteLine($"{Tim.Er}Inside foreach 3: thread={Thread.CurrentThread.ManagedThreadId}, value={n}");
            await Task.Delay(1).ConfigureAwait(false);
        }
        
        Console.Out.WriteLine($"{Tim.Er}After foreach 3 s: thread={Thread.CurrentThread.ManagedThreadId}");
    }
}