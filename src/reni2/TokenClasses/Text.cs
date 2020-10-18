using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Parser;
using Reni.SyntaxTree;

namespace Reni.TokenClasses
{
    sealed class Text : TerminalSyntaxToken
    {
        protected override Result Result(ContextBase context, Category category, TerminalSyntax token)
        {
            var data = Lexer.Instance.ExtractText(token.Token);
            return context
                .RootContext.BitType.Array(BitsConst.BitSize(data[0].GetType()))
                .TextItem
                .Array(data.Length)
                .TextItem
                .Result
                (category, () => CodeBase.BitsConst(BitsConst.ConvertAsText(data)), Closures.Void);
        }

        public override string Id => "<text>";
    }
}