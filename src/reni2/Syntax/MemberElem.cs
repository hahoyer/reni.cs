using HWClassLibrary.Debug;
using Reni.Parser;

namespace Reni.Syntax
{
    /// <summary>
    /// Mamber name and in case of function call an argument list
    /// </summary>
    internal sealed class MemberElem : ReniObject
    {
        private static int _nextObjectId;
        private readonly SyntaxBase _args;
        private readonly DefineableToken _defineableToken;

        public MemberElem(DefineableToken defineableToken, SyntaxBase args) : base(_nextObjectId++)
        {
            _defineableToken = defineableToken;
            _args = args;
            //StopByObjectId(-10);
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
        public SyntaxBase Args { get { return _args; } }

        [DumpData(false)]
        public string FilePosition
        {
            get
            {
                if(_defineableToken != null)
                    return _defineableToken.FilePosition;
                return _args.FilePosition;
            }
        }

        public string DumpShort()
        {
            var result = "";
            if(_defineableToken != null)
                result = _defineableToken.Name;
            if(_defineableToken != null && _args != null)
                result += "(";
            if(_args != null)
                result += _args.DumpShort();
            if(_defineableToken != null && _args != null)
                result += ")";
            return result;
        }
    }
}