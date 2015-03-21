using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Helper;
using hw.Parser;
using hw.Scanner;
using Reni.Basics;
using Reni.Context;
using Reni.ReniParser;
using Reni.ReniSyntax;
using Reni.Struct;
using Reni.TokenClasses;

namespace Reni.Validation
{
    sealed class CompileSyntaxError : CompileSyntax
    {
        [EnableDump]
        readonly IssueId _issueId;
        readonly CompileSyntaxError _previous;
        readonly ValueCache<Issue> _issueCache;

        public CompileSyntaxError
            (IssueId issueId, SourcePart source, CompileSyntaxError previous = null)
        {
            _issueId = issueId;
            _previous = previous;
            _issueCache = new ValueCache<Issue>(() => new Issue(_issueId, source, ""));

            StopByObjectIds(67);
        }

        internal override bool IsError => true;
        [DisableDump]
        internal override IEnumerable<Issue> DirectIssues => Issue.plus(base.DirectIssues);
        [DisableDump]
        protected override IEnumerable<Syntax> DirectChildren { get { yield return _previous; } }

        [DisableDump]
        Issue Issue => _issueCache.Value;

        [DisableDump]
        IEnumerable<CompileSyntaxError> Chain
        {
            get
            {
                var current = this;
                do
                {
                    yield return current;
                    current = current._previous;
                } while(current != null);
            }
        }

        internal override Result ResultForCache(ContextBase context, Category category)
        {
            var result = Chain
                .Select(error => error.IssueType(context).Result(category))
                .Aggregate(context.RootContext.VoidType.Result(category), (x, y) => x + y);
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
        internal override IEnumerable<Syntax> ToList(List type)
        {
            NotImplementedMethod(type);
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

        internal override Syntax SyntaxError(IssueId issue, SourcePart token, Syntax right = null)
        {
            if(right == null)
                return new CompileSyntaxError(issue, token, this);
            NotImplementedMethod(issue, token, right);
            return null;
        }
    }
}