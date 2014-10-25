using System.Linq;
using System.Collections.Generic;
using System;
using hw.Parser;
using hw.Scanner;
using Reni.Parser;
using Reni.Type;
using Reni.Validation;

namespace Reni.Context
{
    sealed class UndefinedSymbolIssue : SyntaxIssue
    {
        readonly string _targetIdentifier;

        UndefinedSymbolIssue(SourcePart position, string targetIdentifier)
            : base(position, IssueId.UndefinedSymbol)
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

        internal static IssueType Type(SourcePart position, TypeBase target)
        {
            return new IssueType(new UndefinedSymbolIssue(position, "Type: "+ target.DumpPrintText), target.RootContext);
        }
        public static IssueType Type(SourcePart position, ContextBase target)
        {
            return new IssueType(new UndefinedSymbolIssue(position, "Context: "+ target.DumpPrintText), target.RootContext);
        }
    }

    sealed class AmbiguousSymbolIssue : SyntaxIssue
    {
        AmbiguousSymbolIssue(SourcePart position)
            : base(position, IssueId.UndefinedSymbol)
        {
            StopByObjectId(266);
        }

        internal static IssueType Type(SourcePart position, Root rootContext)
        {
            return new IssueType(new AmbiguousSymbolIssue(position), rootContext);
        }
    }
}