// MIT License
// Copyright (c) 2024 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using JetBrains.Annotations;

namespace AwaitThreading.Core;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public readonly struct ParallelValueTaskMethodBuilder<T>
{
    private readonly ParallelTaskImpl<T> _parallelTaskImpl = new();

    public ParallelValueTaskMethodBuilder()
    {
    }

    public static ParallelValueTaskMethodBuilder<T> Create() => new();

    public ParallelValueTask<T> Task => new(_parallelTaskImpl);

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
    {
        _parallelTaskImpl.SetStateMachine(ref stateMachine);
    }

    public void SetStateMachine(IAsyncStateMachine stateMachine)
    {
    }

    public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : INotifyCompletion
        where TStateMachine : IAsyncStateMachine
    {
        ParallelTaskMethodBuilderImpl.AwaitOnCompleted(ref awaiter, ref stateMachine);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : ICriticalNotifyCompletion
        where TStateMachine : IAsyncStateMachine
    {
        ParallelTaskMethodBuilderImpl.AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine);
    }

    public void SetResult(T result)
    {
        _parallelTaskImpl.SetResult(new ParallelTaskResult<T>(result));
    }

    public void SetException(Exception exception)
    {
        _parallelTaskImpl.SetResult(new ParallelTaskResult<T>(ExceptionDispatchInfo.Capture(exception)));
    }
}