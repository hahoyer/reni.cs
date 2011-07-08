using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using HWClassLibrary.Helper;
using Reni.Parser;

namespace Reni.Proof
{
    internal abstract class PairSyntax : ParsedSyntax, IComparableEx<PairSyntax>
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

        [DisableDump]
        internal override Set<string> Variables
        {
            get
            {
                if(Operator.IsVariablesProvider)
                    return Left.Variables | Right.Variables;
                return new Set<string>();
            }
        }

        int IComparableEx<PairSyntax>.CompareToEx(PairSyntax other)
        {
            var result = Left.CompareTo(other.Left);
            if(result == 0)
            {
                result = Right.CompareTo(other.Right);
            }
            return result;
        }

        internal override bool IsDistinct(ParsedSyntax other) { return IsDistinct((PairSyntax) other); }
        private bool IsDistinct(PairSyntax other) { return other.Operator != Operator || ParsedSyntaxExtender.IsDistinct(other.Left, Left) || ParsedSyntaxExtender.IsDistinct(other.Right, Right); }
        internal override string SmartDump(ISmartDumpToken @operator) { return Operator.SmartDump(Left, Right); }

        internal override Set<ParsedSyntax> Replace(IEnumerable<KeyValuePair<string, ParsedSyntax>> definitions)
        {
            bool trace = false;
            var leftResults = Left.Replace(definitions);
            var rightResults = Right.Replace(definitions);
            StartMethodDump(trace, definitions,"leftResults",leftResults,"rightResults",rightResults);
            var result = leftResults
                .SelectMany(left => rightResults.Select(syntax => left.Pair(Operator, syntax)))
                .ToSet();
            return ReturnMethodDump(result);
        }
    }

    internal interface IPair
    {
        bool IsVariablesProvider { get; }
        string SmartDump(ParsedSyntax left, ParsedSyntax right);
        ParsedSyntax IsolateClause(string variable, ParsedSyntax left, ParsedSyntax right);
        ParsedSyntax Pair(ParsedSyntax left, ParsedSyntax right);
    }
}