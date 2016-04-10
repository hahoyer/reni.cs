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

        Result<Value> ITokenClass.GetValue(Syntax left, SourcePart token, Syntax right)
            => GetValue(left, token, right);

        virtual protected Result<Value> GetValue(Syntax left, SourcePart token, Syntax right)
        {
            var leftValue = left?.Value;
            var rightValue = right?.Value;
            if((left == null || leftValue != null) && (right == null || rightValue != null))
            {
                var result = CheckedInfix(leftValue?.Target, token, rightValue?.Target);
                if(result != null)
                    return result.With(rightValue?.Issues.plus(leftValue?.Issues));
            }

            if(left == null)
                return Prefix(token, right);

            if(right == null)
                return Suffix(left, token);

            if(leftValue == null && rightValue != null)
                return Infix(left, token, rightValue.Target)
                    .With(rightValue.Issues);

            NotImplementedMethod(left, token, right);
            return null;
        }

        protected virtual Result<Value> Infix(Syntax left, SourcePart token, Value right)
        {
            NotImplementedMethod(left, token, right);
            return null;
        }

        protected virtual Result<Value> Suffix(Syntax left, SourcePart token)
        {
            NotImplementedMethod(left, token);
            return null;
        }

        protected virtual Result<Value> Prefix(SourcePart token, Syntax right)
        {
            NotImplementedMethod(token, right);
            return null;
        }

        Result<Value> CheckedInfix(Value left, SourcePart token, Value right)
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

        protected virtual Result<Value> Infix(Value left, SourcePart token, Value right)
        {
            NotImplementedMethod(left, token, right);
            return null;
        }

        protected virtual Result<Value> Suffix(Value left, SourcePart token)
        {
            NotImplementedMethod(left, token);
            return null;
        }

        protected virtual Result<Value> Prefix(SourcePart token, Value right)
        {
            NotImplementedMethod(token, right);
            return null;
        }

        protected virtual Result<Value> Terminal(SourcePart token)
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