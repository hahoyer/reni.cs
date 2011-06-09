using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;

namespace Reni.Parser
{
    internal sealed class Token : ReniObject
    {
        private readonly TokenData _data;
        private readonly ITokenClass _tokenClass;

        internal Token(SourcePosn source, int length, ITokenClass tokenClass)
        {
            _data = new TokenData(source.Clone(), length);
            _tokenClass = tokenClass;
            source.Incr(length);
        }

        internal TokenData Data { get { return _data; } }

        internal ITokenClass TokenClass { get { return _tokenClass; } }

        [DisableDump]
        public new string NodeDump { get { return ToString(); } }

        public override string ToString() { return Data.Source.FilePosn(Data.Name); }

        public string ShortDump() { return Data.Name; }
        public override string DumpData() { return Data.Name; }

        [DisableDump]
        internal string PrioTableName { get { return TokenClass.PrioTableName(Data.Name); } }

        internal IParsedSyntax Syntax(IParsedSyntax left, IParsedSyntax right) { return TokenClass.Syntax(left, Data, right); }
    }

    internal sealed class TokenData : ReniObject
    {
        private static int _nextObjectId;
        private readonly int _length;
        private readonly SourcePosn _source;

        internal TokenData(SourcePosn source, int length)
            : base(_nextObjectId++)
        {
            _source = source;
            _length = length;
        }

        [DisableDump]
        internal SourcePosn Source { get { return _source; } }

        [DisableDump]
        internal int Length { get { return _length; } }

        [DisableDump]
        internal string Name { get { return _source.SubString(0, _length); } }

        internal string FilePosition { get { return "\n" + Source.FilePosn(Name); } }
    }
}