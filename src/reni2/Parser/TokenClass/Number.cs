using HWClassLibrary.Debug;
using Reni.Code;
using Reni.Context;
using Reni.Syntax;
using Reni.Type;

namespace Reni.Parser.TokenClass
{
    /// <summary>
    /// DigitChain token
    /// </summary>
    internal sealed class Number : Terminal
    {
        internal static readonly TokenClassBase Instance = new Number();

        internal override IParsedSyntax CreateSyntax(IParsedSyntax left, Token token, IParsedSyntax right)
        {
            return CreateTerminalSyntax(left, token, right);
        }

        internal override string DumpShort()
        {
            return "<number>";
        }

        internal override Result Result(ContextBase context, Category category, Token token)
        {
            var bitsConst = BitsConst.Convert(token.Name);
            return TypeBase
                .CreateBit
                .CreateSequence(bitsConst.Size.ToInt())
                .CreateResult(category, () => CodeBase.CreateBitArray(bitsConst));
        }
    }
}