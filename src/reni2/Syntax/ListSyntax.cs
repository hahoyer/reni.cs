using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using Reni.Parser;
using Reni.Struct;

namespace Reni.Syntax
{
    internal sealed class ListSyntax : ReniParser.ParsedSyntax
    {
        private readonly ReniParser.ParsedSyntax _left;
        private readonly ReniParser.ParsedSyntax _right;

        public ListSyntax(ReniParser.ParsedSyntax left, TokenData token, ReniParser.ParsedSyntax right)
            : base(token)
        {
            _left = left ?? new EmptyList(token, token);
            _right = right ?? new EmptyList(token, token);
        }

        internal override string GetNodeDump() { return "(" + _left.GetNodeDump() + ", " + _right.GetNodeDump() + ")"; }

        internal override ReniParser.ParsedSyntax SurroundedByParenthesis(TokenData leftToken, TokenData rightToken)
        {
            var list = new List<IParsedSyntax>();
            IParsedSyntax next = this;
            do
            {
                var current = (ListSyntax) next;
                list.Add(current._left);
                next = current._right;
            } while(next is ListSyntax);

            list.Add(next);

            return Container.Create(leftToken, rightToken, list);
        }
    }
}