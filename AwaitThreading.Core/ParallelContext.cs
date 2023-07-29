//MIT License
//Copyright (c) 2023 Saltuk Konstantin
//See the LICENSE file in the project root for more information.

namespace AwaitThreading.Core;

public readonly struct ParallelContext
{
    [ThreadStatic]
    private static Stack<ParallelContext>? _parallelContexts;

    public ParallelContext(int id)
    {
        Id = id;
    }

    public int Id { get; }

    public static ParallelContext? GetCurrentContext()
    {
        return _parallelContexts?.Peek();
    }

    public static void PushContext(ParallelContext context)
    {
        _parallelContexts ??= new Stack<ParallelContext>();
        _parallelContexts.Push(context);
    }

    public static ParallelContext PopContext()
    {
        return _parallelContexts!.Pop();
    }
}