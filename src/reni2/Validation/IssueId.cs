using System;
using System.Collections.Generic;
using System.Linq;
using hw.Helper;

namespace Reni.Validation
{
    sealed class IssueId : EnumEx
    {
        public static readonly IssueId BeginOfComment = new IssueId();
        public static readonly IssueId ConsequentialError = new IssueId();
        public static readonly IssueId EOFInComment = new IssueId();
        public static readonly IssueId EOFInLineComment = new IssueId();
        public static readonly IssueId EOLInString = new IssueId();
        public static readonly IssueId MissingFunctionGetter = new IssueId();
        public static readonly IssueId MissingLeftOperand = new IssueId();
        public static readonly IssueId MissingRightOperand = new IssueId();
        public static readonly IssueId UndefinedSymbol = new IssueId();
        public static readonly IssueId UnexpectedLeftOperand = new IssueId();
        public static readonly IssueId UnexpectedRightOperand = new IssueId();
        public static readonly IssueId UnexpectedSyntaxError = new IssueId();
    }
}