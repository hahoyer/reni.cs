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

        [IsDumpEnabled(false)]
        bool IAssociative.IsVariablesProvider { get { return true; } }
        [IsDumpEnabled(false)]
        ParsedSyntax IAssociative.Empty { get { return TrueSyntax.Instance; } }
        AssociativeSyntax IAssociative.Syntax(TokenData token, Set<ParsedSyntax> x) { return new AndSyntax(this, token, x); }
        ParsedSyntax IAssociative.Combine(ParsedSyntax left, ParsedSyntax right) { return null; }
        bool IAssociative.IsEmpty(ParsedSyntax parsedSyntax) { return parsedSyntax is TrueSyntax; }
        string IAssociative.SmartDump(Set<ParsedSyntax> set) { return SmartDump(this, set); }

        string ISmartDumpToken.SmartDumpListDelim(ParsedSyntax parsedSyntax, bool isFirst) { return isFirst ? "" : " & "; }
        [IsDumpEnabled(false)]
        bool ISmartDumpToken.IsIgnoreSignSituation { get { return false; } }

    }
}