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
    sealed class EmptyList : Value
    {
        internal override SourcePart Token { get; }

        public EmptyList(SourcePart token)
        {
            Token = token;
            StopByObjectIds();
        }

        protected override string GetNodeDump() => "()";

        internal override Checked<DeclaratorTags> ExclamationSyntax(SourcePart token)
            => new Checked<DeclaratorTags>
                (
                new DeclaratorTags(null, token),
                IssueId.MissingDeclarationTag.CreateIssue(Token));


        internal override Checked<DeclaratorTags> Combine(DeclaratorTags syntax)
            => new Checked<DeclaratorTags>
                (syntax, IssueId.UnexpectedDeclarationTag.CreateIssue(Token));

        internal override Result ResultForCache(ContextBase context, Category category)
            => context.RootContext.VoidType.Result(category);
    }
}