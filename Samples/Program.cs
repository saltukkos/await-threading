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
        ForkAndJoin().GetResult();
        Foreach().GetResult();
        Foreach2().GetResult();
    }

    private static async ParallelTask<int> ForkAndJoin()
    {
        Console.Out.WriteLine($"Before fork: thread={Thread.CurrentThread.ManagedThreadId}");
        await new ForkingTask(2);
        Console.Out.WriteLine($"After fork: thread={Thread.CurrentThread.ManagedThreadId}");
        await ForkTwice();
        Console.Out.WriteLine($"After fork twice: thread={Thread.CurrentThread.ManagedThreadId}");
        await JoinTwice();
        Console.Out.WriteLine($"After join twice: thread={Thread.CurrentThread.ManagedThreadId}");
        await new JoiningTask();
        Console.Out.WriteLine($"After last join: thread={Thread.CurrentThread.ManagedThreadId}");

        return default;
    }
    
    private static async ParallelTask<int> ForkTwice()
    {
        await new ForkingTask(3);
        await new ForkingTask(2);

        return default;
    }

    private static async Task JoinTwice()
    {
        await new JoiningTask();
        await new JoiningTask();
    }

    private static async ParallelTask<int> Foreach()
    {
        var a = new List<int> {1, 2, 3, 4, 5, 6, 7, 8, 9};
        
        Console.Out.WriteLine($"Before foreach: thread={Thread.CurrentThread.ManagedThreadId}");
        
        await foreach (var n in await a.AsParallelAsync(3))
        {
            Console.Out.WriteLine($"Inside foreach: thread={Thread.CurrentThread.ManagedThreadId}, value={n}");
            Thread.Sleep(1);
        }
        
        Console.Out.WriteLine($"After foreach: thread={Thread.CurrentThread.ManagedThreadId}");
        return default;
    }

    private static async ParallelTask<int> Foreach2()
    {
        var a = new List<int> {1, 2, 3, 4, 5, 6, 7, 8, 9};
        
        Console.Out.WriteLine($"Before foreach: thread={Thread.CurrentThread.ManagedThreadId}");
        
        await foreach (var n in a.AsParallel(3))
        {
            Console.Out.WriteLine($"Inside foreach: thread={Thread.CurrentThread.ManagedThreadId}, value={n}");
            Thread.Sleep(1);
        }
        
        Console.Out.WriteLine($"After foreach: thread={Thread.CurrentThread.ManagedThreadId}");
        return default; //TODO to Task<void>
    }
}