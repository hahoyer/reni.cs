using System;
using Reni.Context;
using Reni.Syntax;

namespace Reni.Parser.TokenClass
{
    /// <summary>
    /// Left parenthesis' 
    /// </summary>
    internal sealed class LPar : TokenClassBase
    {
        private readonly int _level;

        private static readonly TokenClassBase _parenthesis = new LPar(3);
        private static readonly TokenClassBase _bracket = new LPar(2);
        private static readonly TokenClassBase _brace = new LPar(1);
        private static readonly TokenClassBase _frame = new LPar(0);

        internal static TokenClassBase Parenthesis { get { return _parenthesis; } }
        internal static TokenClassBase Bracket { get { return _bracket; } }
        internal static TokenClassBase Brace { get { return _brace; } }
        internal static TokenClassBase Frame { get { return _frame; } }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="level">0 for start of file, 1 for "{", 2 for "[" and 3 for "("</param>
        private LPar(int level)
        {
            _level = level;
        }

        private Result ErrorVisit(ContextBase e, Category c, bool match, SyntaxBase left, SyntaxBase right)
        {
            NotImplementedFunction(e, c, match, left, right);
            throw new NotImplementedException();
        }

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
            ParsedSyntax.IsNull(left);
            if(right == null)
                return new EmptyList(token);
            return right.SurroundedByParenthesis(token);
        }

        /// <summary>
        /// Special name for begin of file
        /// </summary>
        internal override string PrioTableName(string name)
        {
            if(_level == 0)
                return "<frame>";

            return base.PrioTableName(name);
        }
    }
}