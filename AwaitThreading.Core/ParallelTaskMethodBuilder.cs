// MIT License
// Copyright (c) 2023 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace AwaitThreading.Core;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public readonly struct ParallelTaskMethodBuilder
{
    public ParallelTaskMethodBuilder()
    {
    }

    public static ParallelTaskMethodBuilder Create() => new();

    public ParallelTask Task { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; } = new();

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
    {
        stateMachine.MoveNext();
    }

    public void SetStateMachine(IAsyncStateMachine stateMachine)
    {
    }

    public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : INotifyCompletion
        where TStateMachine : IAsyncStateMachine
    {
        var parallelTaskImpl = Task.Implementation;
        ParallelTaskMethodBuilderImpl.AwaitOnCompleted(ref awaiter, ref stateMachine, ref parallelTaskImpl);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : ICriticalNotifyCompletion
        where TStateMachine : IAsyncStateMachine
    {
        var parallelTaskImpl = Task.Implementation;
        ParallelTaskMethodBuilderImpl.AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine, ref parallelTaskImpl);
    }

    public void SetResult()
    {
        Task.SetResult();
    }

    public void SetException(Exception exception)
    {
        Task.SetException(exception);
    }
}