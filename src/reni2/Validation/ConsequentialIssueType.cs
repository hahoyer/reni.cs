using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;
using Reni.Code;
using Reni.Context;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Validation
{
    sealed class ConsequentialIssueType : IssueType
    {
        readonly IssueType _issueType;

        public ConsequentialIssueType(IssueType issueType, SourcePart token)
            : base(new Issue(IssueId.ConsequentialError, token)) => _issueType = issueType;

        [DisableDump]
        internal override Root Root => _issueType.Root;

        [DisableDump]
        internal override IEnumerable<string> DeclarationOptions
            => base.DeclarationOptions.Concat(InternalDeclarationOptions);

        [DisableDump]
        IEnumerable<string> InternalDeclarationOptions
        {
            get
            {
                NotImplementedMethod();
                return null;
            }
        }

        internal override CodeBase Code => _issueType.Code + base.Code;
    }

    class IssueInCompoundType : IssueType
    {
        [EnableDump]
        readonly IssueType IssueType;

        public IssueInCompoundType(IssueType issueType)
            : base(new Issue(IssueId.IssueInCompound, issueType.RecentIssue.Position)) 
            => IssueType = issueType;

        [DisableDump]
        internal override Root Root => IssueType.Root;

        protected override TypeBase ReversePair(TypeBase first)
        {
            Tracer.Assert(!(first is IssueType));
            return new IssueInCompoundType(IssueType);
        }

        internal override TypeBase Pair(TypeBase second)
        {
            NotImplementedMethod(second);
            return base.Pair(second);
        }

    }
}