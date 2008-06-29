using HWClassLibrary.Debug;
using Reni.Parser.TokenClass;

namespace Reni.Parser
{
    internal sealed class DefineableToken : ReniObject
    {
        private readonly int _length;
        private readonly SourcePosn _source;
        private readonly Defineable _tokenClass;

        internal DefineableToken(Token token)
        {
            _source = token.Source;
            _length = token.Length;
            _tokenClass = (Defineable) token.TokenClass;
        }

        /// <summary>
        /// Gets the token class.
        /// </summary>
        /// <value>The token class.</value>
        /// created 01.04.2007 23:41 on SAPHIRE by HH
        internal Defineable TokenClass { get { return _tokenClass; } }

        /// <summary>
        /// the text of the token
        /// </summary>
        [DumpData(false)]
        internal string Name { get { return _source.SubString(0, _length); } }

        [DumpData(true)]
        public string FilePosition { get { return "\n" + _source.FilePosn(Name); } }
    }
}