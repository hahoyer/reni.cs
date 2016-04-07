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

        protected virtual Checked<OldSyntax> OldTerminal(SourcePart token)
        {
            NotImplementedMethod(token);
            return null;
        }

        protected virtual Checked<OldSyntax> OldPrefix(SourcePart token, OldSyntax right)
        {
            NotImplementedMethod(token,right);
            return null;
        }

        protected virtual Checked<OldSyntax> OldSuffix(OldSyntax left, SourcePart token)
        {
            NotImplementedMethod(left, token);
            return null;
        }

        protected virtual Checked<OldSyntax> OldInfix
            (OldSyntax left, SourcePart token, OldSyntax right)
        {
            NotImplementedMethod(left, token, right);
            return null;
        }

        internal virtual bool IsVisible => true;

        Checked<Value> ITokenClass.GetValue(Syntax left, IToken token, Syntax right)
        {
            var leftValue = left?.Value;
            if(left != null && leftValue == null)
                return null;

            var rightValue = right?.Value;
            if(right != null && rightValue == null)
                return null;

            return leftValue == null
                ? rightValue == null
                    ? Terminal(token.Characters)
                    : Prefix(token, rightValue.Value)
                        .With(rightValue.Issues)
                : rightValue == null
                    ? Suffix(leftValue.Value, token.Characters)
                        .With(leftValue.Issues)
                    : Infix(leftValue.Value, token, rightValue.Value)
                        .With(rightValue.Issues)
                        .With(leftValue.Issues);
        }

        protected virtual Checked<Value> Infix(Value left, IToken token, Value right)
        {
            NotImplementedMethod(left,token,right);
            return null;
        }

        protected virtual Checked<Value> Suffix(Value left, SourcePart token)
        {
            NotImplementedMethod(left, token);
            return null;
        }

        protected virtual Checked<Value> Prefix(IToken token, Value right)
        {
            NotImplementedMethod(token, right);
            return null;
        }

        protected virtual Checked<Value> Terminal(SourcePart token)
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