//MIT License
//Copyright (c) 2023 Saltuk Konstantin
//See the LICENSE file in the project root for more information.

using System.Reflection;
using System.Runtime.CompilerServices;

namespace AwaitThreading.Core.Tasks;

internal static class StateMachineExtensions
{
    private static readonly MethodInfo CloneMethod =
        typeof(object).GetMethod("MemberwiseClone", BindingFlags.NonPublic | BindingFlags.Instance)!;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TStateMachine MakeCopy<TStateMachine>(this TStateMachine stateMachine)
        where TStateMachine : IAsyncStateMachine
    {
        if (typeof(TStateMachine).IsValueType)
        {
            // in release mode this method should be inlined with no copy overhead at all
            return stateMachine;
        }

        return (TStateMachine)Copy(stateMachine);
    }

    
    private static object Copy(object originalObject)
    {
        return CloneMethod.Invoke(originalObject, null)!; //TODO to compiled
    }
}