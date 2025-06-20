// MIT License
// Copyright (c) 2025 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using JetBrains.Annotations;

namespace AwaitThreading.Core.Tasks;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public readonly struct ParallelValueTaskMethodBuilder
{
    private readonly ParallelTaskImpl<Unit> _parallelTaskImpl = new();

    public ParallelValueTaskMethodBuilder()
    {
    }

    public static ParallelValueTaskMethodBuilder Create() => new();

    public ParallelValueTask Task => new(_parallelTaskImpl);

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

    public void SetResult()
    {
        _parallelTaskImpl.SetResult(new ParallelTaskResult<Unit>(new Unit()));
    }

    public void SetException(Exception exception)
    {
        _parallelTaskImpl.SetResult(new ParallelTaskResult<Unit>(ExceptionDispatchInfo.Capture(exception)));
    }
}