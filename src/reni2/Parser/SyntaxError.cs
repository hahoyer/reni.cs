using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;

namespace Reni.Parser
{
    /// <summary>
    ///     Error token to singal syntax errors
    /// </summary>
    internal sealed class SyntaxError : ReniObject, ITokenClass
    {
        private string _message;

        /// <summary>
        ///     ctor
        /// </summary>
        /// <param name = "message">the error message</param>
        public SyntaxError(string message) { _message = message; }

        string ITokenClass.Name { set { throw new NotImplementedException(); } }
        string ITokenClass.PrioTableName(string name) { throw new NotImplementedException(); }
        ITokenFactory ITokenClass.NewTokenFactory { get { throw new NotImplementedException(); } }
        IParsedSyntax ITokenClass.Syntax(IParsedSyntax left, TokenData token, IParsedSyntax right) { throw new NotImplementedException(); }
    }
}