using System;
using HWClassLibrary.Debug;
using Reni.Context;

namespace Reni.Parser.TokenClass
{
    /// <summary>
    /// Left parenthesis' 
    /// </summary>
    internal class LPar : Base
    {
        private readonly int _level;

        private static readonly Base _parenthesis = new LPar(3);
        private static readonly Base _bracket = new LPar(2);
        private static readonly Base _brace = new LPar(1);
        private static readonly Base _frame = new LPar(0);

        internal static Base Parenthesis { get { return _parenthesis; } }
        internal static Base Bracket { get { return _bracket; } }
        internal static Base Brace { get { return _brace; } }
        internal static Base Frame { get { return _frame; } }
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="level">0 for start of file, 1 for "{", 2 for "[" and 3 for "("</param>
        private LPar(int level)
        {
            _level = level;
        }

        private Result ErrorVisit(Context.Base e, Category c, bool match, Syntax.Base left, Syntax.Base right)
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
        internal override Syntax.Base CreateSyntax(Syntax.Base left, Token token, Syntax.Base right)
        {
            if (left != null)
                return base.CreateSyntax(left, token, right);
            if (right == null)
                return new Syntax.Void(token);
            return right.SurroundedByParenthesis();
        }

        /// <summary>
        /// Special name for begin of file
        /// </summary>
        internal override string PrioTableName(string name)
        {
            if (_level == 0)
                return "<frame>";

            return base.PrioTableName(name);
        }

    }
}