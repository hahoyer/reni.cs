using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using HWClassLibrary.Helper;
using Reni.Parser;

namespace Reni.Proof
{
    internal abstract class AssociativeSyntax : ParsedSyntax
    {
        internal readonly IAssociative Operator;
        internal readonly Set<ParsedSyntax> Set;

        public AssociativeSyntax(IAssociative @operator, TokenData token, Set<ParsedSyntax> set)
            : base(token)
        {
            Operator = @operator;
            Set = set;
        }

        [DisableDump]
        internal override sealed Set<string> Variables
        {
            get
            {
                if(Operator.IsVariablesProvider)
                    return Set<string>.Create(Set.SelectMany(x => x.Variables).ToArray());
                return new Set<string>();
            }
        }

        internal override sealed bool IsDistinct(ParsedSyntax other) { return IsDistinct((AssociativeSyntax) other); }
        internal override string SmartDump(ISmartDumpToken @operator) { return Operator.SmartDump(Set); }

        private bool IsDistinct(AssociativeSyntax other)
        {
            if(other.Operator != Operator)
                return true;
            return other.Set.IsDistinct(Set);
        }
    }

    internal interface ISmartDumpToken
    {
        string SmartDumpListDelim(ParsedSyntax parsedSyntax, bool isFirst);
        bool IsIgnoreSignSituation { get; }
    }

    internal interface IAssociative
    {
        bool IsVariablesProvider { get; }
        ParsedSyntax Empty { get; }
        string SmartDump(Set<ParsedSyntax> set);
        AssociativeSyntax Syntax(TokenData token, Set<ParsedSyntax> x);
        ParsedSyntax Combine(ParsedSyntax left, ParsedSyntax right);
        bool IsEmpty(ParsedSyntax parsedSyntax);
    }
}