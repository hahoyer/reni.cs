using System.Collections.Generic;
using System.Linq;
using System;
using hw.Debug;
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
    abstract class TokenClass : TokenClass<Syntax>, IOperator<Syntax>, IPropertyProvider
    {
        protected override sealed Syntax Create(Syntax left, IToken token, Syntax right)
        {
            var tokenSourcePart = token.SourcePart;
            if(left != null)
                Tracer.Assert
                    (
                        left.SourcePart < tokenSourcePart,
                        left.SourcePart.NodeDump + " < " + tokenSourcePart.NodeDump
                    );
            if(right != null)
                Tracer.Assert(tokenSourcePart < right.SourcePart);

            var result = this.Operation(left, token, right)
                .CheckedSurround(left, left?.SourcePart)
                .CheckedSurround(this, tokenSourcePart)
                .CheckedSurround(right, right?.SourcePart)
                ;

            var resultSourcePart = result.SourcePart;
            var leftSourcePart = (left?.SourcePart ?? tokenSourcePart);
            var isLeftAligned = leftSourcePart.Start == resultSourcePart.Start;
            var rightSourcePart = (right?.SourcePart ?? tokenSourcePart);
            var isRightAligned = rightSourcePart.End == resultSourcePart.End;

            if(isLeftAligned && isRightAligned)
                return result;

            NotImplementedMethod(left, token, right, nameof(result), result);
            var pp = result.SourceParts;
            return result;
        }

        internal Syntax CreateForVisit(Syntax left, IToken token, Syntax right)
            => this.Operation(left, token, right);

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