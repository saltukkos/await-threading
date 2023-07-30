//MIT License
//Copyright (c) 2023 Saltuk Konstantin
//See the LICENSE file in the project root for more information.

namespace AwaitThreading.Core;

public readonly struct ParallelContext
{
    private static readonly AsyncLocal<Stack<ParallelContext>> ParallelContexts = new();

    public ParallelContext(int id)
    {
        Id = id;
    }

    public int Id { get; }

    public static ParallelContext? GetCurrentContext()
    {
        return ParallelContexts.Value?.Peek();
    }

    public static void PushContext(ParallelContext context)
    {
        ParallelContexts.Value ??= new Stack<ParallelContext>();
        ParallelContexts.Value.Push(context);
    }

    public static ParallelContext PopContext()
    {
        return ParallelContexts.Value!.Pop();
    }
}