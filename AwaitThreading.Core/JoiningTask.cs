//MIT License
//Copyright (c) 2023 Saltuk Konstantin
//See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;

namespace AwaitThreading.Core;

public sealed class JoiningTask
{
    public readonly struct JoiningTaskAwaiter : ICriticalNotifyCompletion
    {
        public JoiningTaskAwaiter()
        {
        }

        public bool IsCompleted => false;
    
        public void OnCompleted(Action continuation)
        {
            var context = ParallelContext.PopContext();
            if (context.Id == 0)
            {
                continuation.Invoke();
            }
        }

        public void UnsafeOnCompleted(Action continuation)
        {
            OnCompleted(continuation);
        }

        public void GetResult()
        {
        }
    }

    private readonly JoiningTaskAwaiter _awaiter = new();
    public JoiningTaskAwaiter GetAwaiter() => _awaiter;
}