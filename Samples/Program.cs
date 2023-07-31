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

    private static async ParallelTask ForkAndJoin()
    {
        var random = new Random(Seed: 42);
        Console.Out.WriteLine($"Before fork: thread={Thread.CurrentThread.ManagedThreadId}");
        await new ForkingTask(2);
        Thread.Sleep(random.Next() % 100);
        Console.Out.WriteLine($"After fork: thread={Thread.CurrentThread.ManagedThreadId}");
        await ForkTwice();
        Thread.Sleep(random.Next() % 100);
        Console.Out.WriteLine($"After fork twice: thread={Thread.CurrentThread.ManagedThreadId}");
        await JoinTwice();
        Thread.Sleep(random.Next() % 100);
        Console.Out.WriteLine($"After join twice: thread={Thread.CurrentThread.ManagedThreadId}");
        await new JoiningTask();
        Console.Out.WriteLine($"After last join: thread={Thread.CurrentThread.ManagedThreadId}");
    }
    
    private static async ParallelTask ForkTwice()
    {
        await new ForkingTask(3);
        await new ForkingTask(2);
    }

    private static async ParallelTask JoinTwice()
    {
        await new JoiningTask();
        await new JoiningTask();
    }

    private static async ParallelTask Foreach()
    {
        var a = new List<int> {1, 2, 3, 4, 5, 6, 7, 8, 9};
        
        Console.Out.WriteLine($"Before foreach: thread={Thread.CurrentThread.ManagedThreadId}");
        
        await foreach (var n in await a.AsParallelAsync(3))
        {
            Console.Out.WriteLine($"Inside foreach: thread={Thread.CurrentThread.ManagedThreadId}, value={n}");
            Thread.Sleep(1);
        }
        
        Console.Out.WriteLine($"After foreach: thread={Thread.CurrentThread.ManagedThreadId}");
    }

    private static async ParallelTask Foreach2()
    {
        var a = new List<int> {1, 2, 3, 4, 5, 6, 7, 8, 9};
        
        Console.Out.WriteLine($"Before foreach: thread={Thread.CurrentThread.ManagedThreadId}");
        
        await foreach (var n in a.AsParallel(3))
        {
            Console.Out.WriteLine($"Inside foreach: thread={Thread.CurrentThread.ManagedThreadId}, value={n}");
            Thread.Sleep(1);
        }
        
        Console.Out.WriteLine($"After foreach: thread={Thread.CurrentThread.ManagedThreadId}");
    }
}