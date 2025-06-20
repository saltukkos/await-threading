//MIT License
//Copyright (c) 2023 Saltuk Konstantin
//See the LICENSE file in the project root for more information.

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace AwaitThreading.Core.Context;

public readonly struct ParallelContext : IEquatable<ParallelContext>
{
    private readonly ImmutableStack<ParallelFrame>? _stack;

    private ParallelContext(ImmutableStack<ParallelFrame> stack)
    {
        _stack = stack;
    }

    [MemberNotNullWhen(false, nameof(_stack))]
    public bool IsEmpty => _stack is null || _stack.IsEmpty;

    [Pure]
    public ParallelFrame GetTopFrame()
    {
        if (IsEmpty)
        {
            throw new InvalidOperationException("There are no frames in Parallel Context.");
        }

        return _stack.Peek();
    }

    [Pure]
    public ParallelContext PushFrame(ParallelFrame frame)
    {
        var newStack = (_stack ?? ImmutableStack<ParallelFrame>.Empty).Push(frame);
        return new ParallelContext(newStack);
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ParallelContext PopFrame(out ParallelFrame poppedFrame)
    {
        var currentContextStack = _stack;
        if (currentContextStack is null)
        {
            throw new InvalidOperationException("Stack is empty");
        }

        var newStack = currentContextStack.Pop(out poppedFrame);
        return newStack.IsEmpty ? default : new ParallelContext(newStack);
    }

    [Pure]
    internal string StackToString()
    {
        return _stack is null 
            ? "empty" 
            : string.Join(", ", _stack.Select(t => $"{t.Id}"));
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