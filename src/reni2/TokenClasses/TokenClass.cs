using System.Collections.Generic;
using System.Linq;
using System;
using hw.Helper;
using hw.Parser;
using JetBrains.Annotations;
using Reni.ReniParser;
using Reni.Validation;

namespace Reni.TokenClasses
{
    /// <summary>
    ///     Base class for compiler tokens
    /// </summary>
    abstract class TokenClass : TokenClass<Syntax>, IOperator<Syntax>
    {
        protected override sealed Syntax Create(Syntax left, Token token, Syntax right)
            => this.Operation(left, token, right);

        internal Syntax CreateForVisit(Syntax left, Token token, Syntax right)
            => this.Operation(left, token, right);

        Syntax IOperator<Syntax>.Terminal(Token token)
            => Terminal(token);
        Syntax IOperator<Syntax>.Prefix(Token token, Syntax right)
            => Prefix(token, right);
        Syntax IOperator<Syntax>.Suffix(Syntax left, Token token)
            => Suffix(left, token);
        Syntax IOperator<Syntax>.Infix(Syntax left, Token token, Syntax right)
            => Infix(left, token, right);

        protected virtual Syntax Terminal(Token token)
            => new Validation.SyntaxError(IssueId.UnexpectedSyntaxError, token);

        protected virtual Syntax Prefix(Token token, Syntax right)
        {
            NotImplementedMethod(token, right);
            return null;
        }

        protected virtual Syntax Suffix(Syntax left, Token token)
        {
            NotImplementedMethod(left, token);
            return null;
        }

        protected virtual Syntax Infix(Syntax left, Token token, Syntax right)
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