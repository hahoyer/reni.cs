using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;
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

        protected override Syntax Prefix(SourcePart token, Syntax right)
        {
            return new FunctionSyntax
                (
                token,
                null,
                _isImplicit,
                _isMetaFunction,
                right.ToCompiledSyntax
                );
        }

        protected override Syntax Infix(Syntax left, SourcePart token, Syntax right)
        {
            return new FunctionSyntax
                (
                token,
                left.ToCompiledSyntax,
                _isImplicit,
                _isMetaFunction,
                right.ToCompiledSyntax
                );
        }
    }
}