using System;
using System.Collections.Generic;
using System.Linq;
using hw.Helper;
using hw.Scanner;
using Reni.Parser;
using Reni.TokenClasses;

namespace Reni.Validation
{
    sealed class IssueId : EnumEx
    {
        public static readonly IssueId ConsequentialError = new IssueId();
        public static readonly IssueId EOFInComment = new IssueId();
        public static readonly IssueId EOFInLineComment = new IssueId();
        public static readonly IssueId EOLInString = new IssueId();
        public static readonly IssueId ExtraRightBracket = new IssueId();
        public static readonly IssueId IdentifierExpected = new IssueId();
        public static readonly IssueId InvalidExpression = new IssueId();
        public static readonly IssueId MissingFunctionGetter = new IssueId();
        public static readonly IssueId MissingRightBracket = new IssueId();
        public static readonly IssueId MissingThen = new IssueId();
        public static readonly IssueId MissingElseBody = new IssueId();
        public static readonly IssueId MissingValueInDeclaration = new IssueId();
        public static readonly IssueId UndefinedSymbol = new IssueId();
        public static readonly IssueId UnexpectedDeclarationTag = new IssueId();
        public static readonly IssueId UnexpectedUseAsInfix = new IssueId();
        public static readonly IssueId UnexpectedUseAsPrefix = new IssueId();
        public static readonly IssueId UnexpectedUseAsSuffix = new IssueId();
        public static readonly IssueId UnexpectedUseAsTerminal = new IssueId();

        internal Issue CreateIssue(SourcePart token) => new Issue(this, token, "");
        internal Checked<Syntax> Syntax(SourcePart token)
            => new Checked<Syntax>(new EmptyList(), new Issue(this, token, ""));

        internal Checked<Syntax> Syntax(SourcePart token, Syntax value)
            => new Checked<Syntax>(value, new Issue(this, token, ""));

        internal Checked<Syntax> Syntax(SourcePart token, Syntax value1, Syntax value2)
            => new Checked<Syntax>(ListSyntax.Create(value1, value2), new Issue(this, token, ""));
    }
}