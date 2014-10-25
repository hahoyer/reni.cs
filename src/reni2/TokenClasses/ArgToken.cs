using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;
using Reni.Basics;
using Reni.Context;
using Reni.ReniSyntax;

namespace Reni.TokenClasses
{
    sealed class ArgToken : NonSuffix
    {
        public const string Id = "^";
        public override Result Result(ContextBase context, Category category, SourcePart token)
        {
            return context
                .ArgReferenceResult(category);
        }
        internal override CompileSyntax Visit(ISyntaxVisitor visitor) { return visitor.Arg; }

        public override Result Result(ContextBase context, Category category, SourcePart token, CompileSyntax right)
        {
            return context
                .FunctionalArgResult(category, right);
        }
    }
}