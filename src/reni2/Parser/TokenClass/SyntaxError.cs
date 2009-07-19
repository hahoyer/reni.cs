using System;

namespace Reni.Parser.TokenClass
{
    /// <summary>
    /// Error token to singal syntax errors
    /// </summary>
    internal sealed class SyntaxError : TokenClassBase
    {
        string _message;
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="message">the error message</param>
        public SyntaxError(string message)
        {
            _message = message;
            Name = "<error>"; 
        } 
    }
}
