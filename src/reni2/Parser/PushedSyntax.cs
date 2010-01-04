using Reni.Syntax;

namespace Reni.Parser
{
    /// <summary>
    /// Incomplete syntax tree element 
    /// </summary>
    internal sealed class PushedSyntax : ReniObject
    {
        private readonly IParsedSyntax _left;
        private readonly Token _token;
        internal readonly ITokenFactory TokenFactory;

        internal PushedSyntax(IParsedSyntax left, Token token, ITokenFactory tokenFactory)
        {
            _left = left;
            _token = token;
            TokenFactory = tokenFactory;
        }

        internal char Relation(string newTokenName)
        {
            return TokenFactory.Relation(newTokenName, _token.PrioTableName);
        }

        public IParsedSyntax CreateSyntax(IParsedSyntax args)
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
            return _left.DumpShort() + " " + _token.PrioTableName + " " + TokenFactory;
        }

    }
}