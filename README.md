# AwaitThreading

AwaitThreading is a project dedicated to ease the use of parallel programming using async/await infrastructure of C# programming language.

## Structure
The project consist of two main parts:

- **AwaitThreading.Core:**  This is where the core functionality of the Fork-join model is implemented. It includes `ForkTask` and `JoinTask` that provide the functionality of forking and joining using `await` operator. It utilizes compiler-generated state machine to run parts of async method in parallel. All operations are also available via `ParallelOperations` class. There are also some helper entities (like `ParallelLocal<T>` or `ParallelContextStorage` with `ParallelContext`).

- Some applications of this concept
  - **AwaitThreading.Enumerable:** Contains extension methods for collections, enabling parallel iteration over collection using a simple foreach loop.
  - **To be added:** MPI-like interface, util classes for auto fork\join with `using` construction, etc.

### AwaitThreading.Core

Basic example:
```csharp
using AwaitThreading.Core;
using AwaitThreading.Core.Tasks;

await NormalForkAndJoin(5);

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
```

Methods composition:
```csharp
using AwaitThreading.Core;
using AwaitThreading.Core.Tasks;

await CompositionExample(5);

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
```

### AwaitThreading.Enumerable

There are two main methods: `AsParallel` and `AsParallelAsync`. The key difference is that `AsParallelAsync` performs forking inside it, so after `AsParallelAsync` call caller is already forked, and it requires additional `await` to perform this operation.`AsParallel`, on the other hand, returns `ParallelLazyAsyncEnumerable` and the fork happens inside `ParallelLazyAsyncEnumerator.MoveNextAsync` on the first foreach iteration. This allows caller to not write additional `await` but has slightly more overhead.
```csharp
using AwaitThreading.Core.Tasks;
using AwaitThreading.Enumerable;

await AsParallelAsync();
await AsParallel();

async ParallelTask AsParallelAsync()
{
    var list = Enumerable.Range(1, 10).ToList();
    await foreach (var item in await list.AsParallelAsync(3))
    {
        // foreach body is executed across three separate threads
        Console.Out.WriteLine($"Processing element {item}");
        await Task.Delay(10); //simulate some workload
    }
}

async ParallelTask AsParallel()
{
    var list = Enumerable.Range(1, 10).ToList();
    await foreach (var item in list.AsAsyncParallel(3))
    {
        // foreach body is executed across three separate threads
        Console.Out.WriteLine($"Processing element {item}");
        await Task.Delay(10); //simulate some workload
    }
}
```

## Project status
Project is in development state and not production-ready yet.

TODO list of critical items:
- API is not finalized and provides data to some internal structures (like `ParallelContext`)
- ExecutionContext is not always restored when it has to

## Known limitations
- Exceptions are not propagated from parallel foreach body (compiler-generated state machine saves the exception to a field and rethrows this after `DisposeAsync()`, so there is no way to retrieve this exception for now)