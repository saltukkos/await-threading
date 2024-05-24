// MIT License
// Copyright (c) 2024 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using JetBrains.Annotations;

namespace AwaitThreading.Core;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public struct ParallelValueTaskMethodBuilder<T>
{
    private static readonly ParallelTaskImpl<T> SyncReturnFlag = new();
    private T _result;
    private ParallelTaskImpl<T>? _parallelTaskImpl;

    public static ParallelValueTaskMethodBuilder<T> Create() => default;

    public ParallelValueTask<T> Task
    {
        get
        {
            if (ReferenceEquals(_parallelTaskImpl, SyncReturnFlag))
                return new ParallelValueTask<T>(_result);

            _parallelTaskImpl ??= new ParallelTaskImpl<T>();
            return new ParallelValueTask<T>(_parallelTaskImpl);
        }
    }

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
        ParallelTaskMethodBuilderImpl.AwaitOnCompleted(ref awaiter, ref stateMachine, ref _parallelTaskImpl);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : ICriticalNotifyCompletion
        where TStateMachine : IAsyncStateMachine
    {
        ParallelTaskMethodBuilderImpl.AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine, ref _parallelTaskImpl);
    }

    public void SetResult(T result)
    {
        if (_parallelTaskImpl is null)
        {
            _result = result;
            _parallelTaskImpl = SyncReturnFlag;
            return;
        }

        _parallelTaskImpl ??= new ParallelTaskImpl<T>();
        _parallelTaskImpl.SetResult(new ParallelTaskResult<T>(result));
    }

    public void SetException(Exception exception)
    {
        _parallelTaskImpl ??= new ParallelTaskImpl<T>();
        _parallelTaskImpl.SetResult(new ParallelTaskResult<T>(ExceptionDispatchInfo.Capture(exception)));
    }
}