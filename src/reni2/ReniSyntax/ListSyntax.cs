using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;
using Reni.ReniParser;
using Reni.Struct;

namespace Reni.ReniSyntax
{
    sealed class ListSyntax : Syntax
    {
        readonly Syntax _left;
        readonly Syntax _right;

        public ListSyntax(Syntax left, SourcePart token, Syntax right)
            : base(token)
        {
            _left = left ?? new EmptyList(token);
            _right = right ?? new EmptyList(token);
        }

        protected override string GetNodeDump() { return "(" + _left.NodeDump + ", " + _right.NodeDump + ")"; }

        internal override Syntax RightParenthesis(int level, SourcePart token)
        {
            if(level != 0)
                return base.RightParenthesis(level, token);

            var list = new List<Syntax>();
            Syntax next = this;
            do
            {
                var current = (ListSyntax)next;
                list.Add(current._left);
                next = current._right;
            } while (next is ListSyntax);

            list.Add(next);

            return Container.Create(list[0].Token, token, list);
        }
        internal override Syntax SurroundedByParenthesis(SourcePart leftToken, SourcePart rightToken)
        {
            var list = new List<Syntax>();
            Syntax next = this;
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