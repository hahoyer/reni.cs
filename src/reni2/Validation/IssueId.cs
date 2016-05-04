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
    sealed class IssueId : EnumEx
    {
        public static readonly IssueId AmbigousSymbol = new IssueId();
        public static readonly IssueId ConsequentialError = new IssueId();
        public static readonly IssueId EOFInComment = new IssueId();
        public static readonly IssueId EOFInLineComment = new IssueId();
        public static readonly IssueId EOLInString = new IssueId();
        public static readonly IssueId ExtraLeftBracket = new IssueId();
        public static readonly IssueId ExtraRightBracket = new IssueId();
        public static readonly IssueId IdentifierExpected = new IssueId();
        public static readonly IssueId InvalidExpression = new IssueId();
        public static readonly IssueId InvalidListOperandSequence = new IssueId();
        public static readonly IssueId MissingDeclaration = new IssueId();
        public static readonly IssueId MissingDeclarationTag = new IssueId();
        public static readonly IssueId MissingRightExpression = new IssueId();
        public static readonly IssueId MissingElseBody = new IssueId();
        public static readonly IssueId MissingFunctionGetter = new IssueId();
        public static readonly IssueId MissingThen = new IssueId();
        public static readonly IssueId MissingValueInDeclaration = new IssueId();
        public static readonly IssueId UnexpectedDeclarationTag = new IssueId();
        public static readonly IssueId UnexpectedUseAsInfix = new IssueId();
        public static readonly IssueId UnexpectedUseAsPrefix = new IssueId();
        public static readonly IssueId UnexpectedUseAsSuffix = new IssueId();
        public static readonly IssueId UnexpectedUseAsTerminal = new IssueId();

        public static IEnumerable<IssueId> All => AllInstances<IssueId>();

        internal Issue Create(Syntax syntax) => new Issue(this, syntax);

        internal Result<Value> Value(Syntax syntax)
            => new Result<Value>(new EmptyList(syntax), Create(syntax));

        internal Result<Value> Value(Value value)
            => new Result<Value>(value, Create(value.Syntax));

        internal Result<Syntax> Syntax(Syntax syntax)
            => new Result<Syntax>(syntax, Create(syntax));

        internal RootIssueType Type(Syntax syntax, ContextBase context)
            => new RootIssueType(Create(syntax), context.RootContext);
    }
}