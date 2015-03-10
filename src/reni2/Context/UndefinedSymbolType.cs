using System.Linq;
using System.Collections.Generic;
using System;
using hw.Parser;
using Reni.Parser;
using Reni.Type;
using Reni.Validation;

namespace Reni.Context
{
    sealed class UndefinedSymbolIssue : SyntaxIssue
    {
        readonly string _targetIdentifier;

        UndefinedSymbolIssue(Token token, string targetIdentifier)
            : base(token, IssueId.UndefinedSymbol)
        {
            _targetIdentifier = targetIdentifier;
            StopByObjectId(266);
        }

        internal override string LogDump
        {
            get
            {
                var result = base.LogDump;
                result += " " + _targetIdentifier;
                return result;
            }
        }

        internal static IssueType Type(Token token, TypeBase target)
            =>
                new IssueType
                    (
                    new UndefinedSymbolIssue(token, "Type: " + target.DumpPrintText),
                    target.RootContext);
        public static IssueType Type(Token token, ContextBase target)
            =>
                new IssueType
                    (
                    new UndefinedSymbolIssue(token, "Context: " + target.DumpPrintText),
                    target.RootContext);
    }

    sealed class AmbiguousSymbolIssue : SyntaxIssue
    {
        AmbiguousSymbolIssue(Token token)
            : base(token, IssueId.UndefinedSymbol)
        {
            StopByObjectId(266);
        }

        internal static IssueType Type(Token token, Root rootContext)
            => new IssueType(new AmbiguousSymbolIssue(token), rootContext);
    }
}