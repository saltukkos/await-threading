// MIT License
// Copyright (c) 2023 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

namespace AwaitThreading.Core;

public interface IParallelNotifyCompletion
{
    void ParallelOnCompleted(Action continuation);

    // Sometimes we need continuation to be set in 'ParallelTask' before  '.SetResult()' invocation.
    // It's cruciall to not have race conditions. Consider the next example:
    // async ParallelTask Foo()
    // {
    //     await MethodThatForks();
    //     Console.Out.WriteLine("I am here");
    // }
    // async ParallelTask MethodThatForks()
    // {
    //     await new ForkingTask(2);
    // }
    //
    // In this example, the main thread that calls MethodThatForks, then sees that ForkingAwaiter is not completed
    // and calls 'ParallelTaskMethodBuilder.AwaitUnsafeOnCompleted()'.
    // Here, two tasks are started inside 'ForkingTask.ParallelOnCompleted()'.
    // Then, main thread need to return from the 'MethodThatForks' and handle the fact that returned ParallelTask is 
    // not completed and set the continuation to it. After that, when tasks from ParallelOnCompleted are ready,
    // they will invoke the continuation that contains 'ParallelTask.SetResult()' in the last step of compiler-generated
    // state-machine for the method 'MethodThatForks'. It happens once per task and we will see "I am here" twice in
    // console, since '.SetResult()' will invoke the continuation set by main thread.
    //
    // But it's possible that the main thread hasn't set the continuation to the ParallelTask yet at the moment
    // when tasks from 'ForkingTask' are ready. To handle this situation we block these tasks in
    // 'ParallelTask.SetResult()', untill main thread set the continuation to this task.
    // At the same time, we can't always block the 'ParallelTask.SetResult()' to not fall into a deadlock
    // in synchronous returns:
    // async ParallelTask Foo()
    // {
    //     var result = await ReturnSync();
    // }
    // async ParallelTask<int> ReturnSync()
    // {
    //     return 42;
    // }
    //
    // This flag is designed to differentiate these situation and to track, if the method contains anythig that
    // requires continuation to be set before '.SetResult()' call.
    // This flag leads to 'ParallelTaskAwaiter.RequireContinuationToBeSetBeforeResult' be set
    // and it's checked inside 'ParallelTask.SetResult()'
    bool RequireContinuationToBeSetBeforeResult { get; }
}