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
            : base(Main.TokenFactory.Equal, left, token, right)
        {
            StopByObjectId(461);
        }

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

        protected override ParsedSyntax IsolateClause(string variable)
        {
            if(Left.Variables.Contains(variable))
            {
                Tracer.Assert(!Right.Variables.Contains(variable));
                return Left.IsolateFromEquation(variable, Right);
            }
            Tracer.Assert(Right.Variables.Contains(variable));
            return Right.IsolateFromEquation(variable, Left);
        }
    }
}