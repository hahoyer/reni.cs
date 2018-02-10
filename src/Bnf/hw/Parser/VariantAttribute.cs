using System;
using System.Collections.Generic;
using System.Linq;
using hw.Helper;
using hw.Scanner;
using JetBrains.Annotations;

namespace hw.Parser
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    [MeansImplicitUse]
    sealed class VariantAttribute : Attribute
    {
        internal object[] CreationParameter { get; }

        public VariantAttribute(params object[] creationParameter) { CreationParameter = creationParameter; }

        internal ITokenType CreateInstance(System.Type type)
            => (ITokenType)type
            .GetConstructor(CreationParameter.Select(p => p.GetType()).ToArray())
            .AssertNotNull()
            .Invoke(CreationParameter);
    }
}