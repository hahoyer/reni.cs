using HWClassLibrary.Debug;
using Reni.Parser.TokenClass;

namespace Reni.Parser
{
    sealed public class Token : ReniObject
    {
        private readonly SourcePosn _source;
        private readonly int _length;
        private readonly Base _tokenClass;

        private static int _nextObjectId = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenClass"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="length">The length.</param>
        /// <param name="tokenClass">The tokenClass.</param>
        /// created 31.03.2007 23:27 on SAPHIRE by HH
        public Token(SourcePosn source, int length, TokenClass.Base tokenClass) : base(_nextObjectId++)
        {
            _source = source.Clone();
            _length = length;
            source.Incr(length);
            _tokenClass = tokenClass;
        }

        /// <summary>
        /// Gets the token.
        /// </summary>
        /// <value>The token.</value>
        /// created 31.03.2007 23:28 on SAPHIRE by HH
        public Base TokenClass { get { return _tokenClass; } }

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

        /// <summary>
        /// is like dump
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Source.FilePosn(Name);
        }

        /// <summary>
        /// is like dump
        /// </summary>
        /// <returns></returns>
        public string NodeDump { get { return ToString(); } }

        /// <summary>
        /// is like dump
        /// </summary>
        /// <returns></returns>
        public string FilePosn { get { return "\n" + Source.FilePosn(Name); } }

        /// <summary>
        /// Returns just the name of the token
        /// </summary>
        /// <returns></returns>
        public string ShortDump()
        {
            return Name;
        }

        /// <summary>
        /// Index in priotable. Handles some spacial cases
        /// </summary>
        /// <param name="prioTable"></param>
        /// <returns></returns>
        public int Index(PrioTable prioTable)
        {
            return prioTable.Index(PrioTableName);
        }

        /// <summary>
        /// Normally Name except for some spacial cases
        /// </summary>
        [DumpData(false)]
        public string PrioTableName { get { return TokenClass.PrioTableName(Name); } }

        /// <summary>
        /// Results the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="category">The category.</param>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns></returns>
        internal Result Result(Context.Base context, Category category, Syntax.Base left, Syntax.Base right)
        {
            return TokenClass.Result(left, this, right, context,  category);
        }
    }

    public sealed class DefineableToken: ReniObject
    {
        private readonly SourcePosn _source;
        private readonly int _length;
        private readonly Defineable _tokenClass;

        public DefineableToken(Token token)
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
        public string Name { get { return _source.SubString(0, _length); } }
    }
}
