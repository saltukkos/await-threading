//MIT License
//Copyright (c) 2023 Saltuk Konstantin
//See the LICENSE file in the project root for more information.

using System.Collections.Immutable;

namespace AwaitThreading.Core;

public readonly struct ParallelFrame
{
    public readonly int Id;
    public readonly int Count;
    public readonly SingleWaiterBarrier JoinBarrier;

    public ParallelFrame(int id, int count, SingleWaiterBarrier joinBarrier)
    {
        Id = id;
        Count = count;
        JoinBarrier = joinBarrier;
    }

    public object ForkIdentity => JoinBarrier;
}

public readonly struct ParallelContext
{
    private readonly ImmutableStack<ParallelFrame>? _stack;

    private static readonly AsyncLocal<ParallelContext> CurrentThreadContext = new();

    [ThreadStatic]
    private static int _cachedId; // note: stores ID + 1. 0 means no Id is set  

    private ParallelContext(ImmutableStack<ParallelFrame> stack)
    {
        _stack = stack;
    }

    public static int Id
    {
        get
        {
            var cachedId = _cachedId;
            if (cachedId > 0)
                return cachedId - 1;

            var id = GetCurrentFrame().Id;
            _cachedId = id + 1;
            return id;
        }
    }

    private static ParallelFrame GetCurrentFrame()
    {
        var currentContextStack = CurrentThreadContext.Value._stack;
        if (currentContextStack is null)
            throw new InvalidOperationException("Stack is empty");

        return currentContextStack.Peek();
    }

    public static ParallelFrame? GetCurrentFrameSafe()
    {
        var currentContextStack = CurrentThreadContext.Value._stack;
        if (currentContextStack is null || currentContextStack.IsEmpty)
        {
            return null;
        }

        return currentContextStack.Peek();
    }

    public static void PushFrame(ParallelFrame frame)
    {
        var currentContext = CurrentThreadContext.Value;
        var newStack = (currentContext._stack ?? ImmutableStack<ParallelFrame>.Empty).Push(frame);
        CurrentThreadContext.Value = new ParallelContext(newStack);
        _cachedId = frame.Id + 1;
    }

    public static ParallelFrame PopFrame()
    {
        var currentContext = CurrentThreadContext.Value;
        var currentContextStack = currentContext._stack;
        if (currentContextStack is null)
            throw new InvalidOperationException("Stack is empty");

        var newStack = currentContextStack.Pop(out var poppedFrame);
        CurrentThreadContext.Value = new ParallelContext(newStack);
        ClearCachedId();
        return poppedFrame;
    }

    internal static void ClearCachedId()
    {
        _cachedId = 0;
    }

    internal static string GetCurrentContexts()
    {
        var stack = CurrentThreadContext.Value._stack;
        if (stack is null)
        {
            return "empty";
        }

        return string.Join(", ", stack.Select(t => $"({t.Id} out of {t.Count})"));
    }
}