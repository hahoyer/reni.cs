using hw.DebugFormatter;
using hw.Helper;

namespace hw.Scanner
{
    public sealed class WhiteSpaceTokenType : DumpableObject, ILexerTokenType
    {
        public readonly string Id;
        public WhiteSpaceTokenType(string id) => Id = id;
        string IUniqueIdProvider.Value => Id;
        protected override string GetNodeDump() => Id;
    }
}