using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using HWClassLibrary.Helper;
using Reni.Parser;

namespace Reni.Proof
{
    internal sealed class AssociativeSyntax : ParsedSyntax
    {
        internal readonly IAssociative Operator;
        internal readonly Set<ParsedSyntax> Set;

        public AssociativeSyntax(IAssociative @operator, TokenData token, Set<ParsedSyntax> set)
            : base(token)
        {
            Operator = @operator;
            Set = set;
        }

        internal static Set<ParsedSyntax> ListOf(IAssociative associativeOperator, ParsedSyntax parsedSyntax)
        {
            var commutative = parsedSyntax as AssociativeSyntax;
            if(commutative != null && commutative.Operator == associativeOperator)
                return commutative.Set;
            var result = new Set<ParsedSyntax> {parsedSyntax};
            return result;
        }

        [IsDumpEnabled(false)]
        internal override Set<string> Variables
        {
            get
            {
                if(Operator.IsVariablesProvider)
                    return Set<string>.Create(Set.SelectMany(x => x.Variables).ToArray());
                return new Set<string>();
            }
        }

        internal override bool IsDistinct(ParsedSyntax other) { return IsDistinct((AssociativeSyntax)other); }
        private bool IsDistinct(AssociativeSyntax other)
        {
            if(other.Operator != Operator)
                return true;
            return other.Set.IsDistinct(Set);
        }
    }

    internal interface IAssociative
    {
        bool IsVariablesProvider { get; }
    }

}