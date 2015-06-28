using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;
using Reni.Basics;
using Reni.Context;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.Parser
{
    sealed class EmptyList : CompileSyntax
    {
        readonly SourcePart Token;

        public EmptyList(SourcePart token)
        {
            Token = token;
            StopByObjectIds();
        }

        protected override string GetNodeDump() => "()";

        internal override Checked<ExclamationSyntaxList> Combine(ExclamationSyntaxList syntax)
            => new Checked<ExclamationSyntaxList>
                (syntax, IssueId.UnexpectedDeclarationTag.CreateIssue(Token));

        internal override Result ResultForCache(ContextBase context, Category category)
            => context.RootContext.VoidType.Result(category);
    }
}