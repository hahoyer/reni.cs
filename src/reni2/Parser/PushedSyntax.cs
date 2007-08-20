using Reni.Syntax;

namespace Reni.Parser
{
    /// <summary>
    /// Incomplete syntax tree element 
    /// </summary>
    public class PushedSyntax : ReniObject
    {
        private Base _left;
        private Token _token;

        /// <summary>
        /// ctor
        /// </summary>
        /// 
        /// <param name="left">left syntax element</param>
        /// <param name="token">asis</param>
        public PushedSyntax(Base left, Token token)
        {
            _left = left;
            _token = token;
        }

        /// <summary>
        /// Gets the token.
        /// </summary>
        /// <value>The token.</value>
        /// created 31.03.2007 14:21 on SAPHIRE by HH
        public Token Token { get { return _token; } }

        public Base CreateSyntax(Base args)
        {
            return _token.TokenClass.CreateSyntax(_left, _token, args);
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </returns>
        /// created 09.02.2007 00:10
        public override string ToString()
        {
            if(_left == null)
                return "null " + _token.PrioTableName;
            return _left.DumpShort() + " " + _token.PrioTableName;
        }
    }
}