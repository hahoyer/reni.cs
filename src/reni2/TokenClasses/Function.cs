using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;
using Reni.Parser;
using Reni.Validation;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    [Variant(false, false)]
    [Variant(true, false)]
    [Variant(false, true)]
    sealed class Function : TokenClass,IValueProvider
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

        Result<Value> IValueProvider.Get(Syntax syntax) 
            => FunctionSyntax.Create(syntax.Left, _isImplicit, _isMetaFunction, syntax.Right, syntax);
    }
}