using Reni.Syntax;

namespace Reni.Parser
{
    /// <summary>
    /// Incomplete syntax tree element 
    /// </summary>
    internal sealed class PushedSyntax : ReniObject
    {
        private readonly IParsedSyntax _left;
        internal readonly Token Token;
        internal readonly PrioTable PrioTable;

        internal PushedSyntax(IParsedSyntax left, Token token, PrioTable prioTable)
        {
            _left = left;
            PrioTable = prioTable;
            Token = token;
        }


        public IParsedSyntax CreateSyntax(IParsedSyntax args)
        {
            return Token.TokenClass.CreateSyntax(_left, Token, args);
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
                return "null " + Token.PrioTableName;
            return _left.DumpShort() + " " + Token.PrioTableName;
        }
    }
}