using System.Collections.Generic;
using System.Linq;
using System;
using hw.Debug;
using hw.Helper;
using hw.Parser;
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

            var leftSourcePart = left?.SourcePart;
            var rightSourcePart = right?.SourcePart;

            var checkedLeftSourcePart = (leftSourcePart ?? tokenSourcePart);
            var checkedRightSourcePart = (rightSourcePart ?? tokenSourcePart);

            if(leftSourcePart != null)
                Tracer.Assert
                    (
                        leftSourcePart < tokenSourcePart,
                        leftSourcePart.NodeDump + " < " + tokenSourcePart.NodeDump
                    );
            if(rightSourcePart != null)
                Tracer.Assert(tokenSourcePart < rightSourcePart);

            var rawResult = this.Operation(left, token, right);
            var result = rawResult
                .CheckedSurround(left, leftSourcePart)
                .CheckedSurround(this, tokenSourcePart)
                .CheckedSurround(right, rightSourcePart)
                ;

            var resultSourcePart = result.SourcePart;

            var isLeftAligned = checkedLeftSourcePart.Start == resultSourcePart.Start;
            var isRightAligned = checkedRightSourcePart.End == resultSourcePart.End;

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