using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;
using Reni.ReniParser;
using Reni.Validation;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    [Variant(false, false)]
    [Variant(true, false)]
    [Variant(false, true)]
    sealed class Function : TokenClass
    {
        public static string TokenId(bool isImplicit = false, bool isMetaFunction = false)
            => "/" + (isImplicit ? "!" : "") + "\\" + (isMetaFunction ? "/\\" : "");
        public override string Id => TokenId(_isImplicit, _isMetaFunction);
        readonly bool _isImplicit;
        readonly bool _isMetaFunction;

        public Function(bool isImplicit, bool isMetaFunction)
        {
            _isImplicit = isImplicit;
            _isMetaFunction = isMetaFunction;
        }

        protected override Checked<Syntax> Terminal(SourcePart token)
            => IssueId.MissingFunctionGetter.Syntax(token);
        protected override Checked<Syntax> Suffix(Syntax left, SourcePart token)
            => IssueId.MissingFunctionGetter.Syntax(token, left);

        protected override Checked<Syntax> Prefix(SourcePart token, Syntax right)
            => FunctionSyntax.Create
                (
                    null,
                    _isImplicit,
                    _isMetaFunction,
                    right.ToCompiledSyntax);

        protected override Checked<Syntax> Infix(Syntax left, SourcePart token, Syntax right)
            => FunctionSyntax.Create
                (
                    left.ToCompiledSyntax,
                    _isImplicit,
                    _isMetaFunction,
                    right.ToCompiledSyntax);
    }
}