//MIT License
//Copyright (c) 2023 Saltuk Konstantin
//See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace AwaitThreading.Core;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public readonly struct ParallelTaskMethodBuilder<T>
{
    public ParallelTaskMethodBuilder()
    {
    }

    public static ParallelTaskMethodBuilder<T> Create() => new();

    public ParallelTask<T> Task { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; } = new();

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
    {
        // Do not run! Tasks are 'cold'
        Task.SetStateMachine(ref stateMachine);
    }

    public void SetStateMachine(IAsyncStateMachine stateMachine)
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AwaitOnCompleted<TAwaiter, TStateMachine>(
        ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : INotifyCompletion
        where TStateMachine : IAsyncStateMachine
    {
        ParallelTaskMethodBuilderImpl.AwaitOnCompleted(ref awaiter, ref stateMachine);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(
        ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : ICriticalNotifyCompletion
        where TStateMachine : IAsyncStateMachine
    {
        ParallelTaskMethodBuilderImpl.AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine);
    }

    public void SetResult(T result)
    {
        Task.SetResult(result);
    }

    public void SetException(Exception exception)
    {
        Task.SetException(exception);
    }
}