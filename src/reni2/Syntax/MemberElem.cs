using System;
using Reni.Parser;

namespace Reni.Syntax
{
    /// <summary>
    /// Mamber name and in case of function call an argument list
    /// </summary>
    internal sealed class MemberElem: ReniObject
    {
        private readonly DefineableToken _defineableToken;
        private readonly Base _args;
        private static int _nextObjectId = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberElem"/> class.
        /// </summary>
        /// <param name="defineableToken">The defineable token.</param>
        /// <param name="args">The args.</param>
        /// created 01.04.2007 23:24 on SAPHIRE by HH
        public MemberElem(DefineableToken defineableToken , Base args): base(_nextObjectId++)
        {
            _defineableToken = defineableToken;
            _args = args;
            StopByObjectId(-10);
        }

        /// <summary>
        /// Gets the defineable token.
        /// </summary>
        /// <value>The defineable token.</value>
        /// created 01.04.2007 23:37 on SAPHIRE by HH
        public DefineableToken DefineableToken { get { return _defineableToken; } }
        /// <summary>
        /// Optional argument list
        /// </summary>
        public Base Args { get { return _args; } }

        public string DumpShort()
        {
            string result = "";
            if(_defineableToken != null)
                result = _defineableToken.Name;
            if (_defineableToken != null && _args != null) 
                result += "(";
            if (_args != null)
                result += _args.DumpShort();
            if (_defineableToken != null && _args != null)
                result += ")";
            return result;
        }
    }
}