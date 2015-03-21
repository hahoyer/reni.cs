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
    abstract class TokenClass : TokenClass<SourceSyntax>, IOperator<Syntax>
    {
        protected override sealed SourceSyntax Create
            (SourceSyntax left, IToken token, SourceSyntax right)
        {
            var syntax = this.Operation(left?.Syntax, token, right?.Syntax);
            return new SourceSyntax(syntax, left, token, right);
        }

        internal Syntax CreateForVisit(Syntax left, Syntax right)
            => this.Operation(left, null, right);

        Syntax IOperator<Syntax>.Terminal(IToken token)
            => Terminal(token);
        Syntax IOperator<Syntax>.Prefix(IToken token, Syntax right)
            => Prefix(token, right);
        Syntax IOperator<Syntax>.Suffix(Syntax left, IToken token)
            => Suffix(left, token);
        Syntax IOperator<Syntax>.Infix(Syntax left, IToken token, Syntax right)
            => Infix(left, token, right);

        protected abstract Syntax Terminal(IToken token);

        protected virtual Syntax Prefix(IToken token, Syntax right)
        {
            NotImplementedMethod(token, right);
            return null;
        }

        protected virtual Syntax Suffix(Syntax left, IToken token)
        {
            NotImplementedMethod(left, token);
            return null;
        }

        protected virtual Syntax Infix(Syntax left, IToken token, Syntax right)
        {
            NotImplementedMethod(left, token, right);
            return null;
        }
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

        internal TokenClass CreateInstance(System.Type type)
        {
            return (TokenClass)
                type
                    .GetConstructor(CreationParameter.Select(p => p.GetType()).ToArray())
                    .AssertNotNull()
                    .Invoke(CreationParameter);
        }
    }
}