using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;
using Reni.ReniParser;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    [Variant(false, false)]
    [Variant(true, false)]
    [Variant(false, true)]
    sealed class Function : TokenClass, ITokenClassWithId
    {
        public static string Id(bool isImplicit = false, bool isMetaFunction = false)
            => "/" + (isImplicit ? "!" : "") + (isMetaFunction ? "\\/" : "") + "\\";
        string ITokenClassWithId.Id => Id(_isImplicit, _isMetaFunction);
        readonly bool _isImplicit;
        readonly bool _isMetaFunction;
        public Function(bool isImplicit = false, bool isMetaFunction = false)
        {
            _isImplicit = isImplicit;
            _isMetaFunction = isMetaFunction;
        }

        protected override Syntax Prefix(SourcePart token, Syntax right)
            => new FunctionSyntax
                (
                token,
                null,
                _isImplicit,
                _isMetaFunction,
                right.ToCompiledSyntax
                );

        protected override Syntax Infix(Syntax left, SourcePart token, Syntax right)
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