using System;
using HWClassLibrary.Debug;
using Reni.Code;
using Reni.Context;
using Reni.Type;

namespace Reni.Parser.TokenClass
{
    /// <summary>
    /// DigitChain token
    /// </summary>
    internal sealed class Number : Terminal
    {
        internal static readonly TokenClassBase Instance = new Number();
        internal override string Name { get { return "<number>"; } }

        internal override Result Result(ContextBase context, Category category, Token token)
        {
            var bitsConst = BitsConst.Convert(token.Name);
            return TypeBase
                .CreateBit
                .CreateSequence(bitsConst.Size.ToInt())
                .CreateResult(category, () => CodeBase.CreateBitArray(bitsConst));
        }

        public static Int64 ToInt64(Token token)
        {
            return BitsConst.Convert(token.Name).ToInt64();
        }
    }
}