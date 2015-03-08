using System.Linq;
using System.Collections.Generic;
using System;
using hw.Debug;
using hw.Helper;
using hw.Parser;
using hw.Scanner;
using Reni.Code;
using Reni.Context;

namespace Reni.Validation
{
    public abstract class IssueBase : DumpableObject
    {
        internal static readonly IEnumerable<IssueBase> Empty = new IssueBase[0];
        readonly FunctionCache<Token, ConsequentialError> _consequentialError;
        internal readonly IssueId IssueId;

        internal IssueBase(IssueId issueId)
        {
            IssueId = issueId;
            _consequentialError = new FunctionCache<Token, ConsequentialError>
                (token => new ConsequentialError(token, this));
        }

        internal abstract string LogDump { get; }
        internal ConsequentialError ConsequentialError(Token position)
            => _consequentialError[position];
        protected string Tag => IssueId.Tag;
        internal IssueType Type(Root rootContext) => new IssueType(this, rootContext);
        internal virtual CodeBase Code => CodeBase.Issue(this);
    }
}