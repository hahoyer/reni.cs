using System;
using HWClassLibrary.Debug;
using Reni.Parser.TokenClass;

namespace Reni.Parser
{
    [Serializable]
    internal sealed class Token : ReniObject
    {
        private static int _nextObjectId;
        private readonly int _length;
        private readonly SourcePosn _source;
        private readonly TokenClassBase _tokenClass;

        internal Token(SourcePosn source, int length, TokenClassBase tokenClass) : base(_nextObjectId++)
        {
            _source = source.Clone();
            _length = length;
            source.Incr(length);
            _tokenClass = tokenClass;
        }

        public TokenClassBase TokenClass { get { return _tokenClass; } }

        /// <summary>
        /// the source position the token starts
        /// </summary>
        [DumpData(false)]
        public SourcePosn Source { get { return _source; } }

        /// <summary>
        /// the length in characters
        /// </summary>
        [DumpData(false)]
        public int Length { get { return _length; } }

        /// <summary>
        /// the text of the token
        /// </summary>
        [DumpData(false)]
        public string Name { get { return _source.SubString(0, _length); } }

        public new string NodeDump { get { return ToString(); } }

        public string FilePosition { get { return "\n" + Source.FilePosn(Name); } }

        /// <summary>
        /// Normally Name except for some spacial cases
        /// </summary>
        [DumpData(false)]
        public string PrioTableName { get { return TokenClass.PrioTableName(Name); } }

        public override string ToString()
        {
            return Source.FilePosn(Name);
        }

        /// <summary>
        /// Returns just the name of the token
        /// </summary>
        /// <returns></returns>
        public string ShortDump()
        {
            return Name;
        }

        [DumpData(false)]
        internal TokenFactory NewTokenFactory { get { return TokenClass.NewTokenFactory; } }
    }
}