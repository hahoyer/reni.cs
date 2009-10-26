using System;
using JetBrains.Annotations;

namespace Reni.Parser
{
    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    internal abstract class TokenAttributeBase : Attribute
    {
        internal readonly string Token;

        protected TokenAttributeBase(string token)
        {
            Token = token;
        }

        internal abstract PrioTable CreatePrioTable();
    }
}