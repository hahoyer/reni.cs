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
    abstract class TokenClass
        : ScannerTokenClass,
            IOperator<Syntax>,
            IType<SourceSyntax>
    {
        SourceSyntax IType<SourceSyntax>.Create(SourceSyntax left, IToken token, SourceSyntax right)
            => Create(left, token, right);

        string IType<SourceSyntax>.PrioTableId => Id;
        IType<SourceSyntax> IType<SourceSyntax>.NextTypeIfMatched => NextTypeIfMatched;

        protected virtual IType<SourceSyntax> NextTypeIfMatched => null;

        SourceSyntax Create(SourceSyntax left, IToken token, SourceSyntax right)
        {
            var trace = token.GetObjectId() == -115;
            StartMethodDump(trace, left, token, right);
            try
            {
                BreakExecution();
                var syntax = this.Operation(left?.Syntax, token, right?.Syntax);
                return ReturnMethodDump(new SourceSyntax(syntax, left, token, right));
            }
            finally
            {
                EndMethodDump();
            }
        }

        internal Syntax CreateForVisit(Syntax left, Syntax right)
            => this.Operation(left, null, right);

        Syntax IOperator<Syntax>.Terminal(IToken token)
            => Terminal(token?.Characters);
        Syntax IOperator<Syntax>.Prefix(IToken token, Syntax right)
            => Prefix(token?.Characters, right);
        Syntax IOperator<Syntax>.Suffix(Syntax left, IToken token)
            => Suffix(left, token?.Characters);
        Syntax IOperator<Syntax>.Infix(Syntax left, IToken token, Syntax right)
            => Infix(left, token?.Characters, right);

        protected abstract Syntax Terminal(SourcePart token);
        protected abstract Syntax Prefix(SourcePart token, Syntax right);
        protected abstract Syntax Suffix(Syntax left, SourcePart token);
        protected abstract Syntax Infix(Syntax left, SourcePart token, Syntax right);
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