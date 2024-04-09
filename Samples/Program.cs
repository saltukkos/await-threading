using AwaitThreading.Core;
using AwaitThreading.Enumerable;

await AsParallelAsync();
await AsParallel();

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
    await foreach (var item in list.AsParallel(3))
    {
        Console.Out.WriteLine($"Processing element {item}");
        await Task.Delay(10); //simulate some workload
    }
}