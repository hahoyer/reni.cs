using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using HWClassLibrary.Helper;
using Reni.Parser;

namespace Reni.Proof.TokenClasses
{
    internal sealed class And : TokenClass, IAssociative, ISmartDumpToken
    {
        protected override ParsedSyntax Syntax(ParsedSyntax left, TokenData token, ParsedSyntax right)
        {
            Tracer.Assert(left != null);
            Tracer.Assert(right != null);

            return left.Associative(this, token, right);
        }

        bool IAssociative.IsVariablesProvider { get { return true; } }
        [IsDumpEnabled(false)]
        ParsedSyntax IAssociative.Empty { get { return TrueSyntax.Instance; } }
        AssociativeSyntax IAssociative.Syntax(TokenData token, Set<ParsedSyntax> x) { return new AndSyntax(this, token, x); }
        ParsedSyntax IAssociative.Combine(ParsedSyntax left, ParsedSyntax right) { return null; }
        bool IAssociative.IsEmpty(ParsedSyntax parsedSyntax) { return parsedSyntax is TrueSyntax; }

        string IAssociative.SmartDump(Set<ParsedSyntax> set)
        {
            var i = 0;
            var resultList = set.Aggregate("", (s, x) => s + "\n[" + i++ + "] " + SmartDump(x, false)).Indent();
            return "Clauses:" + resultList;
        }

        string ISmartDumpToken.SmartDumpListDelim(ParsedSyntax parsedSyntax, bool isFirst) { return isFirst ? "" : " & "; }
        [IsDumpEnabled(false)]
        bool ISmartDumpToken.IsIgnoreSignSituation { get { return false; } }

        private string SmartDump(ParsedSyntax x, bool isWatched)
        {
            var result = x.SmartDump(this);
            if(isWatched)
                result += ("\n" + x.Dump() + "\n").Indent(3);
            return result;
        }

    }
}