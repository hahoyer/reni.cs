using System.Linq;
using System.Collections.Generic;
using System;
using hw.Debug;
using hw.Helper;
using hw.Parser;
using Reni.Parser;
using Reni.ReniParser;
using Reni.Validation;

namespace Reni.Context
{
    sealed class UndefinedSymbolIssue : SyntaxIssue

    {
        [EnableDump]
        readonly ContextBase _context;
        readonly ExpressionSyntax _syntax;

        [EnableDump]
        TokenData Token { get { return _syntax.Token; } }

        internal UndefinedSymbolIssue(ContextBase context, ExpressionSyntax syntax)
            : base(syntax, IssueId.UndefinedSymbol)
        {
            _context = context;
            _syntax = syntax;
            StopByObjectId(266);
        }

        internal override string LogDump
        {
            get
            {
                var result = base.LogDump;
                var probes = Probes.Where(p => p.HasImplementations).ToArray();
                if(probes.Length == 0)
                    return result;
                result += ("\n" + probes.Select(x => x.LogDump).Stringify("\n")).Indent();
                return result;
            }
        }

        [DisableDump]
        Probe[] Probes { get { return _syntax.Probes(_context); } }

        internal static IssueType Type(ContextBase context, ExpressionSyntax syntax)
        {
            return new IssueType(new UndefinedSymbolIssue(context, syntax), context.RootContext);
        }
    }
}