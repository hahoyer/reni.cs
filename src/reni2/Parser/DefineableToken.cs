using HWClassLibrary.TreeStructure;
using System;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Parser.TokenClass;

namespace Reni.Parser
{
    [Serializable]
    internal sealed class DefineableToken : ReniObject, IIconKeyProvider
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

        [Node]
        internal Defineable TokenClass { get { return _tokenClass; } }
        [DumpData(false)]
        internal string Name { get { return _source.SubString(0, _length); } }
        [DumpData(true)]
        public string FilePosition { get { return "\n" + _source.FilePosn(Name); } }
        [DumpData(false)]
        public string IconKey { get { return "Symbol"; } }
        [DumpData(false)]
        public override string NodeDump { get { return Name.Quote() + "." + ObjectId; } }
    }
}