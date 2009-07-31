using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Context;
using Reni.Syntax;

namespace Reni.Parser.TokenClass
{
    [Serializable]
    internal sealed class DefinableTokenSyntax : CompileSyntax
    {
        private readonly DefineableToken _defineableToken;

        public DefinableTokenSyntax(Token token)
            : base(token)
        {
            _defineableToken = new DefineableToken(token);
        }

        protected internal override IParsedSyntax CreateDeclarationSyntax(Token token, IParsedSyntax right)
        {
            return new DeclarationSyntax(_defineableToken, token, right);
        }

        [DumpData(false)]
        protected internal override ICompileSyntax ToCompileSyntax { get { return this; } }

        protected internal override Result Result(ContextBase context, Category category)
        {
            NotImplementedMethod(context, category);
            return null;
        }
    }
}