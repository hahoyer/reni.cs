using Reni.Syntax;

namespace Reni.Parser.TokenClass
{
    /// <summary>
    /// Right parenthesis' 
    /// </summary>
    internal sealed class RPar : Base
    {
        private readonly int _level;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="level">0 for end of file, 1 for "{", 2 for "[" and 3 for "("</param>
        internal RPar(int level)
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
        internal override Syntax.Base CreateSyntax(Syntax.Base left, Token token, Syntax.Base right)
        {
            if(right != null)
                return base.CreateSyntax(left, token, right);
            if(left != null)
                return left;
            return new Void(token);
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