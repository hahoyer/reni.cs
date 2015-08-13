using System.Collections.Generic;
using System.Linq;
using System;
using hw.Helper;
using hw.Parser;
using hw.Scanner;
using JetBrains.Annotations;
using Reni.Formatting;
using Reni.Parser;

namespace Reni.TokenClasses
{
    abstract class TokenClass
        : ScannerTokenClass,
            IOperator<Syntax, Checked<Syntax>>,
            IType<SourceSyntax>,
            ITokenClass
    {
        SourceSyntax IType<SourceSyntax>.Create(SourceSyntax left, IToken token, SourceSyntax right)
            => Create(left, token, right);

        string IType<SourceSyntax>.PrioTableId => Id;
        IType<SourceSyntax> IType<SourceSyntax>.NextTypeIfMatched => NextTypeIfMatched;

        protected virtual IType<SourceSyntax> NextTypeIfMatched => null;

        SourceSyntax Create(SourceSyntax left, IToken token, SourceSyntax right)
        {
            var trace = token.Characters.Id == "!";
            StartMethodDump(trace, left, token, right);
            try
            {
                BreakExecution();
                var syntax = this.Operation(left?.Syntax, token, right?.Syntax);
                var result = new SourceSyntax(left, this, token, right, syntax.Value, syntax.Issues);
                syntax.Value.SourcePart = result.SourcePart;
                return ReturnMethodDump(result);
            }
            finally
            {
                EndMethodDump();
            }
        }

        internal Checked<Syntax> CreateForVisit(Syntax left, Syntax right)
            => this.Operation(left, null, right);

        Checked<Syntax> IOperator<Syntax, Checked<Syntax>>.Terminal(IToken token)
            => Terminal(token?.Characters);

        Checked<Syntax> IOperator<Syntax, Checked<Syntax>>.Prefix(IToken token, Syntax right)
            => Prefix(token?.Characters, right);

        Checked<Syntax> IOperator<Syntax, Checked<Syntax>>.Suffix(Syntax left, IToken token)
            => Suffix(left, token?.Characters);

        Checked<Syntax> IOperator<Syntax, Checked<Syntax>>.Infix
            (Syntax left, IToken token, Syntax right)
            => Infix(left, token?.Characters, right);

        protected abstract Checked<Syntax> Terminal(SourcePart token);
        protected abstract Checked<Syntax> Prefix(SourcePart token, Syntax right);
        protected abstract Checked<Syntax> Suffix(Syntax left, SourcePart token);
        protected abstract Checked<Syntax> Infix(Syntax left, SourcePart token, Syntax right);

        internal Formatting.Item ItemFromToken
            (Provider provider, int leadingLineBreaks, int indentLevel, ITokenClass rightTokenClass, IToken token)
        {
            var whiteSpaces = provider.InternalGetWhitespaces
                (
                    rightTokenClass,
                    leadingLineBreaks,
                    indentLevel,
                    token.PrecededWith,
                    this
                );
            var item = new Formatting.Item(whiteSpaces, token);
            return item;
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