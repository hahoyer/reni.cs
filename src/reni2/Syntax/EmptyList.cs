using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Basics;
using Reni.Context;
using Reni.Parser;
using Reni.Type;

namespace Reni.Syntax
{
    internal sealed class EmptyList : CompileSyntax
    {
        private readonly TokenData _rightToken;

        public EmptyList(TokenData leftToken, TokenData rightToken)
            : base(leftToken) { _rightToken = rightToken; }

        protected override TokenData GetLastToken() { return _rightToken; }
        internal override string DumpShort() { return "()"; }

        internal override Result ObtainResult(ContextBase context, Category category) { return context.RootContext.VoidResult(category); }
    }
}