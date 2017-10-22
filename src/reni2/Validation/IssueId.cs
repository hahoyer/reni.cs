using System;
using System.Collections.Generic;
using System.Linq;
using hw.Helper;
using hw.Scanner;
using Reni.Context;
using Reni.Parser;
using Reni.TokenClasses;

namespace Reni.Validation
{
    sealed class IssueId : EnumEx, Match.IError
    {
        public static readonly IssueId AmbigousSymbol = new IssueId();
        public static readonly IssueId ConsequentialError = new IssueId();
        public static readonly IssueId EOFInComment = new IssueId();
        public static readonly IssueId EOFInLineComment = new IssueId();
        public static readonly IssueId EOLInString = new IssueId();
        public static readonly IssueId ExtraLeftBracket = new IssueId();
        public static readonly IssueId ExtraRightBracket = new IssueId();
        public static readonly IssueId InvalidCharacter = new IssueId();
        public static readonly IssueId InvalidDeclarationTag = new IssueId();
        public static readonly IssueId UnknownDeclarationTag = new IssueId();
        public static readonly IssueId InvalidExpression = new IssueId();
        public static readonly IssueId InvalidListOperandSequence = new IssueId();
        public static readonly IssueId MissingDeclarationForType = new IssueId();
        public static readonly IssueId MissingDeclarationInContext = new IssueId();
        public static readonly IssueId MissingDeclarationTag = new IssueId();
        public static readonly IssueId MissingRightExpression = new IssueId();
        public static readonly IssueId TerminalUsedAsSuffix = new IssueId();

        public static IEnumerable<IssueId> All => AllInstances<IssueId>();

        internal Issue Create(SourcePart position) => new Issue(this, position);

        internal Result<Value> Value(Syntax syntax)
            => new Result<Value>(new EmptyList(syntax), Create(syntax.SourcePart));

        internal Result<Value> Value(Value value)
            => new Result<Value>(value, Create(value.SourcePart));

        internal Result<Syntax> Syntax(Syntax syntax)
            => new Result<Syntax>(syntax, Create(syntax.SourcePart));

        internal RootIssueType Type(ISyntax currentTarget, ContextBase context)
            => new RootIssueType(Create(currentTarget.All), context.RootContext);
    }
}