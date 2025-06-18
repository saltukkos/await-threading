using AwaitThreading.Core;
using AwaitThreading.Core.Operations;
using AwaitThreading.Core.Tasks;
using AwaitThreading.Enumerable;
using AwaitThreading.Enumerable.Experimental;

await NormalForkAndJoin(5);
await CompositionExample(5);
await CustomOptions();

await AsParallelAsync();
await AsParallel();
await AsParallelExperimental();

async ParallelTask CompositionExample(int threadsCount)
{
    var id = await ForkAndGetId(threadsCount);
    Console.Out.WriteLine($"Hello world from {id}");
    await JoinInsideMethod();
}

async ParallelTask<int> ForkAndGetId(int threadsCount)
{
    var id = await ParallelOperations.Fork(threadsCount);
    return id;
}

async ParallelTask JoinInsideMethod()
{
    await ParallelOperations.Join();
}


async ParallelTask NormalForkAndJoin(int threadsCount)
{
    Console.Out.WriteLine("Before fork: single thread");

    var id = await ParallelOperations.Fork(threadsCount);
    Console.Out.WriteLine($"Hello world from {id}"); //executed on two different threads

    // any (sync or async) workload
    await Task.Delay(100);
    Thread.Sleep(100);

    await ParallelOperations.Join();
    Console.Out.WriteLine("After join: single thread");
}

async ParallelTask CustomOptions()
{
    var id = await ParallelOperations.Fork(2, new ForkingOptions{TaskCreationOptions = TaskCreationOptions.PreferFairness, TaskScheduler = TaskScheduler.Default});
    Console.Out.WriteLine($"Hello world from {id}");
    await ParallelOperations.JoinOnMainThread();
}

async ParallelTask AsParallelAsync()
{
    var list = Enumerable.Range(1, 10).ToList();
    await foreach (var item in await list.AsParallelAsync(3))
    {
        Console.Out.WriteLine($"Processing element {item}");
        await Task.Delay(10); //simulate some workload
    }
}

async ParallelTask AsParallel()
{
    var list = Enumerable.Range(1, 10).ToList();
    await foreach (var item in list.AsAsyncParallel(3))
    {
        Console.Out.WriteLine($"Processing element {item}");
        await Task.Delay(10); //simulate some workload
    }
}

async ParallelTask AsParallelExperimental()
{
    var list = Enumerable.Range(1, 10).ToList();
    await foreach (var item in list)
    {
        Console.Out.WriteLine($"Processing element {item}");
        await Task.Delay(10); //simulate some workload
    }
}