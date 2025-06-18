using AwaitThreading.Core;
using AwaitThreading.Core.Tasks;
using AwaitThreading.Enumerable;
using AwaitThreading.Enumerable.Experimental;

await MyAsyncMethod();
await AsParallelAsync();
await AsParallel();
await AsParallelExperimental();

async ParallelTask MyAsyncMethod()
{
    var id = await ParallelOperations.Fork(2);
    Console.Out.WriteLine($"Hello World from thread {id}");
    await Task.Delay(100); // any async workload
    Thread.Sleep(100); //any sync workload
    await ParallelOperations.Join();
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