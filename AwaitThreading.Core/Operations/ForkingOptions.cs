// MIT License
// Copyright (c) 2025 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

namespace AwaitThreading.Core.Operations;

public sealed class ForkingOptions
{
    public TaskCreationOptions? TaskCreationOptions { get; set; }
    public TaskScheduler? TaskScheduler { get; set; }
}