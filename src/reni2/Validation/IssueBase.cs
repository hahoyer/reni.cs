using System.Linq;
using System.Collections.Generic;
using System;
using hw.Debug;
using hw.Helper;
using hw.Scanner;
using Reni.Code;
using Reni.Context;

namespace Reni.Validation
{
    public abstract class IssueBase : DumpableObject
    {
        internal static readonly IEnumerable<IssueBase> Empty = new IssueBase[0];
        readonly FunctionCache<SourcePart, ConsequentialError> _consequentialError;
        internal readonly IssueId IssueId;

        internal IssueBase(IssueId issueId)
        {
            IssueId = issueId;
            _consequentialError = new FunctionCache<SourcePart, ConsequentialError>
                (syntax => new ConsequentialError(syntax, this));
        }

        internal abstract string LogDump { get; }
        internal ConsequentialError ConsequentialError(SourcePart position) { return _consequentialError[position]; }
        protected string Tag { get { return IssueId.Tag; } }
        internal IssueType Type(Root rootContext) { return new IssueType(this, rootContext); }
        internal virtual CodeBase Code { get { return CodeBase.Issue(this); } }
    }
}