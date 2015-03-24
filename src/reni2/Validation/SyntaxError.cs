using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Scanner;
using Reni.Basics;
using Reni.Context;
using Reni.ReniParser;
using Reni.ReniSyntax;
using Reni.Struct;
using Reni.TokenClasses;

namespace Reni.Validation
{
    sealed class SyntaxError : CompileSyntax
    {
        static int _nextObjectId;
        readonly Syntax[] _others;

        internal SyntaxError(Issue issue, params Syntax[] others)
            : base(_nextObjectId++)
        {
            _others = others;
            Issue = issue;

            StopByObjectIds();
        }

        [DisableDump]
        Issue Issue { get; }
        internal override bool IsError => true;
        [DisableDump]
        internal override IEnumerable<Issue> DirectIssues => Issue.plus(base.DirectIssues);
        [DisableDump]
        protected override IEnumerable<Syntax> DirectChildren => _others;

        internal override ExclamationSyntaxList ExclamationSyntax(ExclamationSyntaxList syntax)
            => syntax.AddError(this);


        internal override Result ResultForCache(ContextBase context, Category category)
        {
            var seed = IssueType(context).Result(category);
            var result = _others
                .Select(item => item as CompileSyntax)
                .Where(item => item != null)
                .Select(item => item.Result(context, category))
                .Aggregate(seed, (x, y) => x + y);
            return result;
        }

        internal override Syntax End => new ListSyntax(null, new[] {this});

        internal override Syntax CreateElseSyntax(CompileSyntax elseSyntax)
        {
            NotImplementedMethod(elseSyntax);
            return null;
        }
        internal override Syntax CreateDeclarationSyntax(SourcePart token, Syntax right)
        {
            NotImplementedMethod(token, right);
            return null;
        }

        [DisableDump]
        internal override CompoundSyntax ToContainer
        {
            get
            {
                NotImplementedMethod();
                return null;
            }
        }

        IssueType IssueType(ContextBase context) => new RootIssueType(Issue, context.RootContext);
    }
}