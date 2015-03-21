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
        public static readonly IssueId MissingRightBracket = new IssueId();
        public static readonly IssueId MissingFunctionGetter = new IssueId();
        public static readonly IssueId MissingLeftOperand = new IssueId();
        public static readonly IssueId UnexpectedUseAsTerminal = new IssueId();
        public static readonly IssueId UndefinedSymbol = new IssueId();
        public static readonly IssueId UnexpectedUseAsSuffix = new IssueId();
        public static readonly IssueId UnexpectedRightOperand = new IssueId();
        public static readonly IssueId UnexpectedSyntaxError = new IssueId();
        public static readonly IssueId UnexpectedUseAsPrefix = new IssueId();
        public static readonly IssueId CompiledSyntaxExpected = new IssueId();
        public static readonly IssueId IdentifierExpected = new IssueId();
        public static readonly IssueId ExtraRightBracket = new IssueId();
        public static readonly IssueId MissingValueInDeclaration = new IssueId();
    }
}