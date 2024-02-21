//MIT License
//Copyright (c) 2023 Saltuk Konstantin
//See the LICENSE file in the project root for more information.

using AwaitThreading.Core;
using AwaitThreading.Enumerable;

namespace Samples;

public class Program
{
    public static async Task Main(string[] args)
    {

        // ThreadPool.SetMinThreads(5, 100);
        Logger.Log("Start!");
        await MinimalExampleWithRequireContinuationToBeSetBeforeResult().WaitAsync();
        await MinimalRepro().WaitAsync();
        // return;
        var result = await JustGiveMeAValueAsyncWrapper().WaitAsync();
        Logger.Log(result.ToString());
        //
        await Foreach3().WaitAsync();
        await Foreach3().WaitAsync();
        //
        await ForkAndJoin().WaitAsync();
        await ForkAndJoinWithAwaits().WaitAsync();
        await Foreach().WaitAsync();
        await Foreach().WaitAsync();
        await Foreach2().WaitAsync();

        Logger.Log("Finish");
    }

    private static async ParallelTask MinimalExampleWithRequireContinuationToBeSetBeforeResult()
    {
        for (var i = 0; i < 10; ++i)
        {
            await MethodThatForks();
            await new JoiningTask();
        }
    }

    private static async ParallelTask MethodThatForks()
    {
        await new ForkingTask(2);
    }
    
    private static async ParallelTask MinimalRepro()
    {
        // for (int i = 0; i < 1; ++i)
        {
            await new ForkingTask(100);
            Logger.Log("Forked!");
            //await Task.Delay(1);
            // Logger.Log(" Delayed! {Thread.CurrentThread.ManagedThreadId}");
            await JoinOnce();
            Logger.Log("Joined!");
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
        
        Logger.Log("After fork twice (before delay)");
        await Task.Yield();
        Logger.Log("After fork twice (after delay)");

        Logger.Log("Before fork");
        await new ForkingTask(2);
        Thread.Sleep(random.Next() % 10);
        Logger.Log("After fork");
        await ForkTwice();
        Thread.Sleep(random.Next() % 10);
        Logger.Log("After fork twice");
        await JoinTwice();
        Thread.Sleep(random.Next() % 10);
        Logger.Log("After join twice");
        await new JoiningTask();
        Logger.Log("After last join");
    }

    private static async ParallelTask ForkAndJoinWithAwaits()
    {
        var random = new Random(Seed: 4242);
        Logger.Log("Before fork");
        await new ForkingTask(2);

        Logger.Log("After fork (before delay 1)");
        // await Task.Delay(random.Next() % 10);
        await Task.Yield();
        Logger.Log("After fork (after delay 1)");
        
        // await ForkTwice();
        await new ForkingTask(2);
        await new ForkingTask(3);
        
        Logger.Log("After fork twice (before delay 2)");
        // await Task.Delay(random.Next() % 10);
        await Task.Yield();
        Logger.Log("After fork twice (after delay 2)");

        await JoinOnce();
        await JoinOnce();
        // await JoinTwice();
        // await new JoiningTask();
        // await new JoiningTask();

        await Task.Delay(random.Next() % 10);
        Logger.Log("After join twice");
        await new JoiningTask();
        Logger.Log("After last join");
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
        
        Logger.Log("Before foreach 1");
        
        await foreach (var n in await a.AsParallelAsync(3))
        {
            Logger.Log($"Inside foreach 1: value={n}");
            Thread.Sleep(1);
        }
        
        Logger.Log("After foreach 1");
    }

    private static async ParallelTask Foreach2()
    {
        var a = new List<int> {1, 2, 3, 4, 5, 6, 7, 8, 9};
        
        Logger.Log("Before foreach 2");
        
        await foreach (var n in a.AsParallel(3))
        {
            Logger.Log($"Inside foreach 2, value={n}");
            Thread.Sleep(1);
        }
        
        Logger.Log("After foreach 2");
    }

    private static async ParallelTask Foreach3()
    {
        var a = new List<int> {1, 2, 3, 4, 5, 6, 7, 8, 9};
        
        Logger.Log("Before foreach 3");
        
        await foreach (var n in a.AsParallel(3))
        {
            Logger.Log($"Inside foreach 3, value={n}");
            await Task.Delay(1).ConfigureAwait(false);
        }
        
        Logger.Log("After foreach 3");
    }
}