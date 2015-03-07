using System.Collections.Generic;
using System.Linq;
using System;
using hw.Helper;
using hw.Parser;
using hw.Scanner;
using JetBrains.Annotations;
using Reni.ReniParser;

namespace Reni.TokenClasses
{
    /// <summary>
    ///     Base class for compiler tokens
    /// </summary>
    abstract class TokenClass : TokenClass<Syntax>, IOperator<Syntax>
    {
        protected override sealed Syntax Create(Syntax left, SourcePart token, Syntax right)
            => this.Operation(left, token, right);

        internal Syntax CreateForVisit(Syntax left, SourcePart token, Syntax right)
            => this.Operation(left, token, right);

        Syntax IOperator<Syntax>.Terminal(SourcePart token) 
            => Terminal(token);
        Syntax IOperator<Syntax>.Prefix(SourcePart token, Syntax right) 
            => Prefix(token, right);
        Syntax IOperator<Syntax>.Suffix(Syntax left, SourcePart token) 
            => Suffix(left, token);
        Syntax IOperator<Syntax>.Infix(Syntax left, SourcePart token, Syntax right)
            => Infix(left, token, right);

        protected virtual Syntax Terminal(SourcePart token)
        {
            NotImplementedMethod(token);
            return null;
        }

        protected virtual Syntax Prefix(SourcePart token, Syntax right)
        {
            NotImplementedMethod(token, right);
            return null;
        }

        protected virtual Syntax Suffix(Syntax left, SourcePart token)
        {
            NotImplementedMethod(left, token);
            return null;
        }

        protected virtual Syntax Infix(Syntax left, SourcePart token, Syntax right)
        {
            NotImplementedMethod(left, token, right);
            return null;
        }
    }

    interface ITokenClassWithId
    {
        string Id { get; }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    [MeansImplicitUse]
    sealed class BelongsTo : Attribute
    {
        public System.Type TokenFactory { get; }

        public BelongsTo(System.Type tokenFactory) { TokenFactory = tokenFactory; }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    [MeansImplicitUse]
    sealed class Variant : Attribute
    {
        object[] CreationParameter { get; }

        public Variant(params object[] creationParameter) { CreationParameter = creationParameter; }

        internal ITokenClassWithId CreateInstance(System.Type type)
        {
            return (ITokenClassWithId)
                type
                    .GetConstructor(CreationParameter.Select(p => p.GetType()).ToArray())
                    .AssertNotNull()
                    .Invoke(CreationParameter);
        }
    }
}