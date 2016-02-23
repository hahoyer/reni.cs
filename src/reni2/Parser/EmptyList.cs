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
        internal override SourcePart Token { get; }

        public EmptyList(SourcePart token)
        {
            Token = token;
            StopByObjectIds();
        }

        protected override string GetNodeDump() => "()";

        internal override Checked<ExclamationSyntaxList> ExclamationSyntax(SourcePart token)
            => new Checked<ExclamationSyntaxList>
                (
                new ExclamationSyntaxList(null, token),
                IssueId.MissingDeclarationTag.CreateIssue(Token));


        internal override Checked<ExclamationSyntaxList> Combine(ExclamationSyntaxList syntax)
            => new Checked<ExclamationSyntaxList>
                (syntax, IssueId.UnexpectedDeclarationTag.CreateIssue(Token));

        internal override Result ResultForCache(ContextBase context, Category category)
            => context.RootContext.VoidType.Result(category);
    }
}