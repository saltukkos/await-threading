//MIT License
//Copyright (c) 2023 Saltuk Konstantin
//See the LICENSE file in the project root for more information.

using System.Reflection;

namespace AwaitThreading.Core;

public static class ObjectExtensions
{
    private static readonly MethodInfo CloneMethod =
        typeof(object).GetMethod("MemberwiseClone", BindingFlags.NonPublic | BindingFlags.Instance)!;

    public static object Copy(this object originalObject)
    {
        return CloneMethod.Invoke(originalObject, null)!; //TODO to compiled
    }
}