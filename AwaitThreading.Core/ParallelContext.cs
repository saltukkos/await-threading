//MIT License
//Copyright (c) 2023 Saltuk Konstantin
//See the LICENSE file in the project root for more information.

using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace AwaitThreading.Core;

public readonly struct ParallelFrame : IEquatable<ParallelFrame>
{
    public readonly int Id;
    public readonly int Count;
    public readonly SingleWaiterBarrier JoinBarrier;

#if DEBUG
    public readonly string CreationStackTrace;
#endif

    public ParallelFrame(int id, int count, SingleWaiterBarrier joinBarrier)
    {
        Id = id;
        Count = count;
        JoinBarrier = joinBarrier;
#if DEBUG
        CreationStackTrace = Environment.StackTrace;
#endif
    }

    public object ForkIdentity => JoinBarrier;

    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(ParallelFrame other)
    {
        return 
            Id == other.Id
            //    &&
            // Count == other.Count
               // &&
               // JoinBarrier.Equals(other.JoinBarrier)
               &&
               CreationStackTrace == other.CreationStackTrace;
    }

    public override bool Equals(object? obj)
    {
        return obj is ParallelFrame other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Count, JoinBarrier, CreationStackTrace);
    }
}

public readonly struct ParallelContext : IEquatable<ParallelContext>
{
    private readonly ImmutableStack<ParallelFrame>? _stack;

    [ThreadStatic]
    private static ParallelContext _currentThreadContext;

    private ParallelContext(ImmutableStack<ParallelFrame> stack)
    {
        _stack = stack;
    }

    public static int Id => GetCurrentFrame().Id;

    [Pure]
    private static ParallelFrame GetCurrentFrame()
    {
        var currentContextStack = _currentThreadContext._stack;
        if (currentContextStack is null || currentContextStack.IsEmpty)
        {
            throw new InvalidOperationException("Stack is empty");
        }

        return currentContextStack.Peek();
    }

    [Pure]
    public static ParallelFrame? GetCurrentFrameSafe()
    {
        var currentContextStack = _currentThreadContext._stack;
        if (currentContextStack is null || currentContextStack.IsEmpty)
        {
            return null;
        }

        return currentContextStack.Peek();
    }

    [Pure]
    public static ParallelContext GetCurrentContext()
    {
        return _currentThreadContext;
    }

    public static void PushFrame(ParallelFrame frame)
    {
        var currentContext = _currentThreadContext;
        var newStack = (currentContext._stack ?? ImmutableStack<ParallelFrame>.Empty).Push(frame);
        _currentThreadContext = new ParallelContext(newStack);
        Logger.Log("Frame pushed");
    }

    public static ParallelFrame PopFrame()
    {
        var currentContext = _currentThreadContext;
        var currentContextStack = currentContext._stack;
        if (currentContextStack is null)
        {
            throw new InvalidOperationException("Stack is empty");
        }

        var newStack = currentContextStack.Pop(out var poppedFrame);
        _currentThreadContext = newStack.IsEmpty ? default : new ParallelContext(newStack);
        Logger.Log("Frame popped");
        return poppedFrame;
    }

    internal static ParallelContext CaptureAndClear()
    {
        var currentContext = _currentThreadContext;
        _currentThreadContext = default;
        Logger.Log("Context cleared");
        return currentContext;
    }

    internal static ParallelContext Capture()
    {
        return _currentThreadContext;
    }

    internal static void Restore(ParallelContext context)
    {
        VerifyContextIsEmpty();
        _currentThreadContext = context;
        Logger.Log("Context restored");
    }

    internal static void RestoreNoVerification(ParallelContext context)
    {
        _currentThreadContext = context;
        Logger.Log("Context restored (no verification)");
    }

    [Conditional("DEBUG")]
    private static void VerifyContextIsEmpty()
    {
        if (_currentThreadContext._stack is not null)
            throw new InvalidOperationException("Context already exists");
    }

    [Pure]
    internal static string GetCurrentContexts()
    {
        return _currentThreadContext.GetCurrentContexts2();
    }

    // TODO: normal methods
    [Pure]
    internal string GetCurrentContexts2()
    {
        var stack = _stack;
        if (stack is null)
        {
            return "empty";
        }

        return string.Join(", ", stack.Select(t => $"({t.Id} out of {t.Count})"));
    }

    public bool IsEmpty => _stack is null || _stack.IsEmpty;

    public bool Equals(ParallelContext other)
    {
        return Equals(_stack, other._stack);
    }

    public override bool Equals(object? obj)
    {
        return obj is ParallelContext other && Equals(other);
    }

    public override int GetHashCode()
    {
        return _stack != null ? _stack.GetHashCode() : 0;
    }
}