using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;
using Reni.ReniParser;

namespace Reni.TokenClasses
{
    sealed class Function : TokenClass
    {
        readonly bool _isImplicit;
        readonly bool _isMetaFunction;
        internal Function(bool isImplicit = false, bool isMetaFunction = false)
        {
            _isImplicit = isImplicit;
            _isMetaFunction = isMetaFunction;
        }

        protected override ParsedSyntax PrefixSyntax(TokenData token, ParsedSyntax right)
        {
            return new FunctionSyntax
                (token
                    , null
                    , _isImplicit
                    , _isMetaFunction
                    , right.ToCompiledSyntax()
                );
        }

        protected override ParsedSyntax InfixSyntax(ParsedSyntax left, TokenData token, ParsedSyntax right)
        {
            return new FunctionSyntax
                (token
                    , left.ToCompiledSyntax()
                    , _isImplicit
                    , _isMetaFunction
                    , right.ToCompiledSyntax()
                );
        }
    }
}