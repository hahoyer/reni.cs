using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using Reni.Parser;

namespace Reni.Proof
{
    internal sealed class EqualSyntax : PairSyntax, IComparableEx<EqualSyntax>
    {
        public EqualSyntax(ParsedSyntax left, TokenData token, ParsedSyntax right)
            : base(Main.TokenFactory.Equal, left, token, right) { }

        int IComparableEx<EqualSyntax>.CompareToEx(EqualSyntax other)
        {
            var result = Left.CompareTo(other.Right);
            if(result == 0)
                return Right.CompareTo(other.Left);

            result = Left.CompareTo(other.Left);
            if(result == 0)
                return Right.CompareTo(other.Right);

            return result;
        }

        internal override KeyValuePair<string, ParsedSyntax>? ToDefinition()
        {
            if(Left.IsSimpleVariable)
            {
                Tracer.Assert(!Right.IsSimpleVariable);
                return new KeyValuePair<string, ParsedSyntax>(Left.Variables.First(), Right);
            }
            if(Right.IsSimpleVariable)
                return new KeyValuePair<string, ParsedSyntax>(Right.Variables.First(), Left);

            return null;
        }
    }
}