//MIT License
//Copyright (c) 2023 Saltuk Konstantin
//See the LICENSE file in the project root for more information.

using System.Collections.Immutable;

namespace AwaitThreading.Core;

public readonly struct ParallelFrame
{
    public readonly int Id;
    public readonly int Count;

    public ParallelFrame(int id, int count)
    {
        Id = id;
        Count = count;
    }
}

public readonly struct ParallelContext
{
    private readonly ImmutableStack<ParallelFrame>? _stack;

    private static readonly AsyncLocal<ParallelContext> CurrentThreadContext = new();

    private ParallelContext(ImmutableStack<ParallelFrame> stack)
    {
        _stack = stack;
    }

    public static ParallelContext CaptureParallelContext()
    {
        return CurrentThreadContext.Value;
    }

    public static void SetParallelContext(ParallelContext value)
    {
        CurrentThreadContext.Value = value;
    }

    public static ParallelFrame GetCurrentFrame()
    {
        var currentContextStack = CurrentThreadContext.Value._stack;
        if (currentContextStack is null) 
            throw new InvalidOperationException("Stack is empty");

        return currentContextStack.Peek();
    }

    public static void PushFrame(ParallelFrame frame)
    {
        var currentContext = CurrentThreadContext.Value;
        var newStack = (currentContext._stack ?? ImmutableStack<ParallelFrame>.Empty).Push(frame);
        CurrentThreadContext.Value = new ParallelContext(newStack);
    }

    public static ParallelFrame PopFrame()
    {
        var currentContext = CurrentThreadContext.Value;
        var currentContextStack = currentContext._stack;
        if (currentContextStack is null) 
            throw new InvalidOperationException("Stack is empty");

        var newStack = currentContextStack.Pop(out var poppedFrame);
        CurrentThreadContext.Value = new ParallelContext(newStack);
        return poppedFrame;
    }

    public static string GetCurrentContexts()
    {
        var stack = CurrentThreadContext.Value._stack;
        if (stack is null)
        {
            return "empty";
        }

        return string.Join(", ", stack.Select(t => $"({t.Id} out of {t.Count})"));
    }
}