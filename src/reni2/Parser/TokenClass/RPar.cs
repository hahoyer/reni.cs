using System;
using Reni.Syntax;

namespace Reni.Parser.TokenClass
{
    /// <summary>
    /// Right parenthesis' 
    /// </summary>
    [Serializable]
    internal sealed class RPar : TokenClassBase
    {

        private readonly int _level;

        private static readonly TokenClassBase _parenthesis = new RPar(3);
        private static readonly TokenClassBase _bracket = new RPar(2);
        private static readonly TokenClassBase _brace = new RPar(1);
        private static readonly TokenClassBase _frame = new RPar(0);

        internal static TokenClassBase Parenthesis { get { return _parenthesis; } }
        internal static TokenClassBase Bracket { get { return _bracket; } }
        internal static TokenClassBase Brace { get { return _brace; } }
        internal static TokenClassBase Frame { get { return _frame; } }
        internal override string Name { get { return "<right " + _level + ">"; } }

        private RPar(int level)
        {
            _level = level;
        }

        /// <summary>
        /// True for end of file
        /// </summary>
        /// <returns></returns>
        internal override bool IsEnd { get { return _level == 0; } }

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
            if(right != null)
                return base.CreateSyntax(left, token, right);
            if(left != null)
                return left;
            return new EmptyList(token);
        }

        /// <summary>
        /// Special name for end of file
        /// </summary>
        internal override string PrioTableName(string name)
        {
            if(_level == 0)
                return "<end>";

            return base.PrioTableName(name);
        }
    }
}