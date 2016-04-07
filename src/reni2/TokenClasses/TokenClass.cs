using System;
using System.Collections.Generic;
using System.Linq;
using hw.Helper;
using hw.Parser;
using hw.Scanner;
using JetBrains.Annotations;
using Reni.Parser;

namespace Reni.TokenClasses
{
    abstract class TokenClass
        : ScannerTokenClass,
            IType<SourceSyntax>,
            ITokenClass
    {
        SourceSyntax IType<SourceSyntax>.Create(SourceSyntax left, IToken token, SourceSyntax right)
            => Create(left, token, right);

        string IType<SourceSyntax>.PrioTableId => Id;

        SourceSyntax Create(SourceSyntax left, IToken token, SourceSyntax right)
            => SourceSyntax.CreateSourceSyntax(left, this, token, right);

        protected abstract Checked<Syntax> OldTerminal(SourcePart token);
        protected abstract Checked<Syntax> Prefix(SourcePart token, Syntax right);
        protected abstract Checked<Syntax> Suffix(Syntax left, SourcePart token);
        protected abstract Checked<Syntax> Infix(Syntax left, SourcePart token, Syntax right);

        internal virtual bool IsVisible => true;

        Checked<CompileSyntax> ITokenClass.ToCompiledSyntax(SourceSyntax left, IToken token, SourceSyntax right)
        {
            var leftSyntax = left?.ToCompiledSyntax;
            var rightSyntax = right?.ToCompiledSyntax;
            if(left != null && leftSyntax == null)
                return null;
            if(right != null && rightSyntax == null)
                return null;

            if(leftSyntax == null)
                if(rightSyntax == null)
                    return Terminal(token.SourcePart);
            NotImplementedMethod(left, token, right, nameof(leftSyntax), leftSyntax, nameof(rightSyntax), rightSyntax);
            return null;
        }

        Checked<CompileSyntax> Terminal(SourcePart token)
        {
            NotImplementedMethod(token);
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