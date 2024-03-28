# AwaitThreading

AwaitThreading is a project dedicated to ease the use of parallel programming using async/await infrastructure of C# programming language.

## Structure
The project consist of two main parts:

- **AwaitThreading.Core:**  This is where the core functionality of the Fork-join model is implemented. It includes `ForkTask` and `JoinTask` that provide the functionality of forking and joining using `await` operator. It utilizes compiler-generated state machine to run parts of async method in parallel. 

- Some applications of this concept
  - **AwaitThreading.Enumerable:** Contains extension methods for collections, enabling parallel iteration over collection using a simple foreach loop.
  - **To be added:** MPI-like interface, util classes for auto fork\join with `using` construction, etc.

### AwaitThreading.Core

Basic example:
```csharp
using AwaitThreading.Core;

await NormalForkAndJoin(5).WaitAsync();

async ParallelTask NormalForkAndJoin(int threadsCount)
{
    Console.Out.WriteLine("Before fork: single thread");

    await new ForkingTask(threadsCount);
    var id = ParallelContext.GetCurrentFrame().Id;
    Console.Out.WriteLine($"Hello world from {id}");

    await new JoiningTask();
    Console.Out.WriteLine("After join: single thread");
}
```

Methods composition:
```csharp
using AwaitThreading.Core;

await CompositionExample(5).WaitAsync();

async ParallelTask CompositionExample(int threadsCount)
{
    var id = await ForkAndGetId(threadsCount);
    Console.Out.WriteLine($"Hello world from {id}");
    await JoinInsideMethod();
}

async ParallelTask<int> ForkAndGetId(int threadsCount)
{
    await new ForkingTask(threadsCount);
    return ParallelContext.GetCurrentFrame().Id;
}

async ParallelTask JoinInsideMethod()
{
    await new JoiningTask();
}
```

### AwaitThreading.Enumerable

There are two main methods: `AsParallel` and `AsParallelAsync`. The key difference is that `AsParallelAsync` performs forking inside it, so after `AsParallelAsync` call caller is already forked, and it requires additional `await` to perform this operation.`AsParallel`, on the other hand, returns `ParallelLazyAsyncEnumerable` and the fork happens inside `ParallelLazyAsyncEnumerator.MoveNextAsync` on the first foreach iteration. This allows caller to not write additional `await` but has slightly more overhead.
```csharp
using AwaitThreading.Core;
using AwaitThreading.Enumerable;

await AsParallelAsync().WaitAsync();
await AsParallel().WaitAsync();

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
```

## Project status
Project is in development state and not production-ready yet.

TODO list of critical items:
- Exceptions handling (probably should aggregate all exceptions inside `JoinTask`)
- API is not finalized and provides data to some internal structures (like `ParallelContext`)
- No steps to minimize overhead are made yet: There are a lot of unnecessary allocations