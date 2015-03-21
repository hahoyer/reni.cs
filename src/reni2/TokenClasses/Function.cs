using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;
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

        protected override Syntax Terminal(IToken token)
            => new CompileSyntaxError(IssueId.MissingFunctionGetter);

        protected override Syntax Prefix(IToken token, Syntax right)
            => new FunctionSyntax
                (
                token,
                null,
                _isImplicit,
                _isMetaFunction,
                right.ToCompiledSyntax
                );

        protected override Syntax Infix(Syntax left, IToken token, Syntax right)
            => new FunctionSyntax
                (
                token,
                left.ToCompiledSyntax,
                _isImplicit,
                _isMetaFunction,
                right.ToCompiledSyntax
                );
    }
}