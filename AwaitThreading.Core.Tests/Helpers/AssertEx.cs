// MIT License
// Copyright (c) 2024 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

namespace AwaitThreading.Core.Tests.Helpers;

public static class AssertEx
{
    public static async Task CheckThrowsAsync<TException>(Func<Task> testFunc) where TException : Exception
    {
        try
        {
            await testFunc.Invoke();
        }
        catch (Exception exception)
        {
            Assert.That(exception, Is.InstanceOf<TException>());
            return;
        }
        
        Assert.Fail("No exception is thrown");
    }
}