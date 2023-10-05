// MIT License
// Copyright (c) 2023 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

namespace AwaitThreading.Core;

public interface IParallelNotifyCompletion
{
    void ParallelOnCompleted(Action continuation);
}