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
            IType<Syntax>,
            ITokenClass
    {
        Syntax IType<Syntax>.Create(Syntax left, IToken token, Syntax right)
            => Create(left, token, right);

        string IType<Syntax>.PrioTableId => Id;

        Syntax Create(Syntax left, IToken token, Syntax right)
            => Syntax.CreateSourceSyntax(left, this, token, right);

        protected abstract Checked<OldSyntax> OldTerminal(SourcePart token);
        protected abstract Checked<OldSyntax> Prefix(SourcePart token, OldSyntax right);
        protected abstract Checked<OldSyntax> Suffix(OldSyntax left, SourcePart token);
        protected abstract Checked<OldSyntax> Infix(OldSyntax left, SourcePart token, OldSyntax right);

        internal virtual bool IsVisible => true;

        Checked<Value> ITokenClass.GetValue(Syntax left, IToken token, Syntax right)
        {
            var leftSyntax = left?.Value;
            var rightSyntax = right?.Value;
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

        Checked<Value> Terminal(SourcePart token)
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