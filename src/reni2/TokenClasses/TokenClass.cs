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

        protected virtual Checked<OldSyntax> OldTerminal(SourcePart token)
        {
            NotImplementedMethod(token);
            return null;
        }

        protected virtual Checked<OldSyntax> OldPrefix(SourcePart token, OldSyntax right)
        {
            NotImplementedMethod(token, right);
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

        [DisableDump]
        internal virtual bool IsVisible => true;

        Checked<Value> ITokenClass.GetValue(Syntax left, SourcePart token, Syntax right)
        {
            var leftValue = left?.Value;
            var rightValue = right?.Value;
            if((left == null || leftValue != null) && (right == null || rightValue != null))
            {
                var result = CheckedInfix(leftValue?.Value, token, rightValue?.Value);
                if(result != null)
                    return result.With(rightValue?.Issues.plus(leftValue?.Issues));
            }

            if(left == null)
                return Prefix(token, right);

            if(right == null)
                return Suffix(left, token);

            if(leftValue == null && rightValue != null)
                return Infix(left, token, rightValue.Value)
                    .With(rightValue.Issues);

            NotImplementedMethod(left, token, right);
            return null;
        }

        protected virtual Checked<Value> Infix(Syntax left, SourcePart token, Value right)
        {
            NotImplementedMethod(left, token, right);
            return null;
        }

        protected virtual Checked<Value> Suffix(Syntax left, SourcePart token)
        {
            NotImplementedMethod(left, token);
            return null;
        }

        protected virtual Checked<Value> Prefix(SourcePart token, Syntax right)
        {
            NotImplementedMethod(token, right);
            return null;
        }

        Checked<Value> CheckedInfix(Value left, SourcePart token, Value right)
        {
            var iresult = Infix(left, token, right);
            if(iresult != null)
                return iresult;

            if(left == null)
            {
                var pResult = Prefix(token, right);
                if(pResult != null)
                    return pResult;
            }

            if(right == null)
            {
                var sResult = Suffix(left, token);
                if(sResult != null)
                    return sResult;
            }

            if(left == null && right == null)
            {
                var tResult = Terminal(token);
                if(tResult != null)
                    return tResult;
            }

            return null;
        }

        protected virtual Checked<Value> Infix(Value left, SourcePart token, Value right)
        {
            NotImplementedMethod(left, token, right);
            return null;
        }

        protected virtual Checked<Value> Suffix(Value left, SourcePart token)
        {
            NotImplementedMethod(left, token);
            return null;
        }

        protected virtual Checked<Value> Prefix(SourcePart token, Value right)
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