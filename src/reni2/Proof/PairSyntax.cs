using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using HWClassLibrary.Helper;
using Reni.Parser;

namespace Reni.Proof
{
    internal sealed class PairSyntax : ParsedSyntax
    {
        internal readonly IPair Operator;
        internal readonly ParsedSyntax Left;
        internal readonly ParsedSyntax Right;

        internal PairSyntax(IPair @operator, ParsedSyntax left, TokenData token, ParsedSyntax right)
            : base(token)
        {
            Operator = @operator;
            Left = left;
            Right = right;
        }

        [IsDumpEnabled(false)]
        internal override Set<string> Variables
        {
            get
            {
                if(Operator.IsVariablesProvider)
                    return Left.Variables | Right.Variables;
                return new Set<string>();
            }
        }

        internal override bool IsDistinct(ParsedSyntax other) { return IsDistinct((PairSyntax) other); }
        private bool IsDistinct(PairSyntax other) { return other.Operator != Operator || ParsedSyntaxExtender.IsDistinct(other.Left, Left) || ParsedSyntaxExtender.IsDistinct(other.Right, Right); }
    }

    internal interface IPair
    {
        bool IsVariablesProvider { get; }
    }

}