using hw.Parser;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Parser;

namespace Reni.TokenClasses
{
    sealed class Text : TerminalSyntaxToken
    {
        public override string Id => "<text>";

        protected override Result Result(ContextBase context, Category category, IToken token)
        {
            var data = Lexer.Instance.ExtractText(token.Characters);
            return context
                .RootContext.BitType.Array(BitsConst.BitSize(data[0].GetType()))
                .TextItem
                .Array(data.Length)
                .TextItem
                .Result
                    (category, () => CodeBase.BitsConst(BitsConst.ConvertAsText(data)), Closures.Void);
        }
    }
}