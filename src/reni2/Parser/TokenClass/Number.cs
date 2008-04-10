using HWClassLibrary.Debug;

namespace Reni.Parser.TokenClass
{
    /// <summary>
    /// DigitChain token
    /// </summary>
    internal sealed class Number : Special
    {
        private static readonly Base _instance = new Number();

        /// <summary>
        /// Creates the syntax.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="token">The token.</param>
        /// <param name="right">The right.</param>
        /// <returns></returns>
        /// created 31.03.2007 14:02 on SAPHIRE by HH
        internal override Syntax.Base CreateSyntax(Syntax.Base left, Token token, Syntax.Base right)
        {
            return CreateSpecialSyntax(left, token, right);
        }

        /// <summary>
        /// Results the specified token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="context">The context.</param>
        /// <param name="category">The category.</param>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns></returns>
        /// created 04.05.2007 23:25 on HAHOYER-DELL by hh
        internal override Result Result(Context.Base context, Category category, Syntax.Base left, Token token, Syntax.Base right)
        {
            Tracer.Assert(left == null);
            Tracer.Assert(right == null);
            var bitsConst = BitsConst.Convert(token.Name);
            return Type.Base
                .CreateBit
                .CreateSequence(bitsConst.Size.ToInt())
                .CreateResult(category, delegate { return Code.Base.CreateBitArray(bitsConst); });
        }

        public static Base Instance { get { return _instance; } }
    }
}