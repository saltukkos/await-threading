// MIT License
// Copyright (c) 2024 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

using NUnit.Framework.Constraints;

namespace AwaitThreading.Core.Tests.Helpers;

public static class ConstraintExtensions
{
    public static EqualConstraint UsingStringLinesCountEquality(this EqualConstraint equalConstraint)
    {
        return equalConstraint
            .Using<string>((s1, s2) => s1.Split(Environment.NewLine).Length == s2.Split(Environment.NewLine).Length);
    }

}