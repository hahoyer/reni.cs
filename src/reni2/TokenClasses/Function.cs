using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;
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

        protected override Syntax Terminal(SourcePart token)
            => new CompileSyntaxError(IssueId.MissingFunctionGetter, token);

        protected override Syntax Prefix(SourcePart token, Syntax right)
            => new FunctionSyntax
                (null,
                _isImplicit,
                _isMetaFunction,
                right.ToCompiledSyntax
                );

        protected override Syntax Infix(Syntax left, SourcePart token, Syntax right)
            => new FunctionSyntax
                (left.ToCompiledSyntax,
                _isImplicit,
                _isMetaFunction,
                right.ToCompiledSyntax
                );
    }
}