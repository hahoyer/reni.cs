using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
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

        protected virtual Result<OldSyntax> OldTerminal(SourcePart token)
        {
            NotImplementedMethod(token);
            return null;
        }

        protected virtual Result<OldSyntax> OldPrefix(SourcePart token, OldSyntax right)
        {
            NotImplementedMethod(token, right);
            return null;
        }

        protected virtual Result<OldSyntax> OldSuffix(OldSyntax left, SourcePart token)
        {
            NotImplementedMethod(left, token);
            return null;
        }

        protected virtual Result<OldSyntax> OldInfix
            (OldSyntax left, SourcePart token, OldSyntax right)
        {
            NotImplementedMethod(left, token, right);
            return null;
        }

        [DisableDump]
        internal virtual bool IsVisible => true;

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