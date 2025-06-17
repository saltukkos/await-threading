//MIT License
//Copyright (c) 2023 Saltuk Konstantin
//See the LICENSE file in the project root for more information.

using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

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

    public bool Equals(ParallelFrame other)
    {
        return Id == other.Id && JoinBarrier.Equals(other.JoinBarrier);
    }

    public override bool Equals(object? obj)
    {
        return obj is ParallelFrame other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, JoinBarrier);
    }
}

public readonly struct ParallelContext : IEquatable<ParallelContext>
{
    [ThreadStatic]
    private static ParallelContext _currentThreadContext;

    private readonly ImmutableStack<ParallelFrame>? _stack;

    private ParallelContext(ImmutableStack<ParallelFrame> stack)
    {
        _stack = stack;
    }

    public static ParallelContext CurrentThreadContext
    {
        get => _currentThreadContext;
        internal set => _currentThreadContext = value;
    }

    [MemberNotNullWhen(false, nameof(_stack))]
    public bool IsEmpty => _stack is null || _stack.IsEmpty;

    [Pure]
    public ParallelFrame GetCurrentFrame()
    {
        if (IsEmpty)
        {
            throw new InvalidOperationException("There are no frames in Parallel Context.");
        }

        return _stack.Peek();
    }

    [Pure]
    public ParallelFrame? GetCurrentFrameSafe()
    {
        if (IsEmpty)
        {
            return null;
        }

        return _stack.Peek();
    }

    [Pure]
    public ParallelContext PushFrame(ParallelFrame frame)
    {
        var newStack = (_stack ?? ImmutableStack<ParallelFrame>.Empty).Push(frame);
        return new ParallelContext(newStack);
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
        return poppedFrame;
    }

    internal static ParallelContext CaptureAndClear()
    {
        var currentContext = _currentThreadContext;
        _currentThreadContext = default;
        Logger.Log("Context cleared");
        return currentContext;
    }

    internal static void ClearButNotExpected()
    {
        VerifyContextIsEmpty();
        _currentThreadContext = default;
    }

    internal static void Restore(ParallelContext context)
    {
        VerifyContextIsEmpty();
        _currentThreadContext = context;
        Logger.Log("Context restored");
    }

    [Conditional("DEBUG")]
    private static void VerifyContextIsEmpty()
    {
        if (_currentThreadContext._stack is not null)
            Debug.Fail("Context already exists");
    }

    [Pure]
    internal string StackToString()
    {
        var stack = _stack;
        if (stack is null)
        {
            return "empty";
        }

        return string.Join(", ", stack.Select(t => $"({t.Id} out of {t.Count})"));
    }

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