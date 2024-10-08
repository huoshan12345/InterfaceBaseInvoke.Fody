﻿using System.Reflection;
using InterfaceBaseInvoke.Tests.AssemblyToProcess;
using InterfaceBaseInvoke.Tests.InvalidAssemblyToProcess;

namespace InterfaceBaseInvoke.Tests.Weaving
{
    public abstract class ClassTestsBase
    {
        protected static readonly string VerifiableAssembly = typeof(AssemblyToProcessReference).Assembly.GetName().Name!;
        protected static readonly string InvalidAssembly = typeof(InvalidAssemblyToProcessReference).Assembly.GetName().Name!;

        protected virtual bool NetStandard => false;
        protected abstract string ClassName { get; }

        protected dynamic GetInstance()
        {
            return NetStandard
                ? StandardAssemblyToProcessFixture.TestResult.GetInstance($"{VerifiableAssembly}.{ClassName}")
                : AssemblyToProcessFixture.TestResult.GetInstance($"{VerifiableAssembly}.{ClassName}");
        }

        protected string ShouldHaveError(string methodName)
            => InvalidAssemblyToProcessFixture.ShouldHaveError($"{InvalidAssembly}.{ClassName}", methodName, true);
    }
}
