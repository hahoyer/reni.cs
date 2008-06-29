using System.Collections.Generic;
using Reni.Struct;
using Reni.Syntax;

namespace Reni.Parser.TokenClass
{
    /// <summary>
    /// List token (comma, semicolon)
    /// </summary>
    sealed internal class List : TokenClassBase
    {
        private List() { }
        private static readonly List _instance = new List();

        internal static List Instance{get { return _instance; } }

        /// <summary>
        /// Creates the syntax.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="token">The token.</param>
        /// <param name="right">The right.</param>
        /// <returns></returns>
        /// created 31.03.2007 14:02 on SAPHIRE by HH
        internal override IParsedSyntax CreateSyntax(IParsedSyntax left, Token token, IParsedSyntax right)
        {
            return new ListSyntax(left, token, right);
        }
    }

    sealed internal class ListSyntax : ParsedSyntax
    {
        private readonly IParsedSyntax _left;
        private readonly IParsedSyntax _right;

        public ListSyntax(IParsedSyntax left, Token token, IParsedSyntax right):base(token)
        {
            _left = left ?? new EmptyList(token);
            _right = right ?? new EmptyList(token);
        }

        internal protected override string DumpShort()
        {
            return "("+ _left.DumpShort() + ", " + _right.DumpShort() + ")";
        }

        internal protected override IParsedSyntax SurroundedByParenthesis(Token token)
        {
            var list = new List<IParsedSyntax>();
            IParsedSyntax next = this; 
            do
            {
                var current = (ListSyntax) next;
                list.Add(current._left);
                next = current._right;
            } 
            while(next is ListSyntax);

            list.Add(next);

            return Container.Create(token, list);


        }
    }
}