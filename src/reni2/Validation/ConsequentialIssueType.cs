using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;
using Reni.Code;
using Reni.Context;
using Reni.Type;

namespace Reni.Validation
{
    [Obsolete("", true)]
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
    }

    [Obsolete("", true)]
    class IssueInCompoundType : IssueType
    {
        [EnableDump]
        readonly IssueType IssueType;

        public IssueInCompoundType(IssueType issueType, SourcePart position)
            : base(new Issue(IssueId.IssueInCompound, position))
            => IssueType = issueType;

        [DisableDump]
        internal override Root Root => IssueType.Root;

        protected override TypeBase ReversePair(TypeBase first)
        {
            Tracer.Assert(!(first is IssueType));
            return new IssueInCompoundType(IssueType, null);
        }

        internal override TypeBase Pair(TypeBase second, SourcePart position)
        {
            NotImplementedMethod(second);
            return base.Pair(second, position);
        }
    }
}