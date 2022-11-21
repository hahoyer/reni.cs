using hw.Scanner;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.DeclarationOptions;
using Reni.Parser;

namespace Reni.TokenClasses;

sealed class Text : TerminalSyntaxToken
{
    static readonly Declaration[] PredefinedDeclarations = { new("dumpprint") };

    protected override Declaration[] Declarations => PredefinedDeclarations;
    public override string Id => "<text>";

    protected override Result Result(ContextBase context, Category category, SourcePart token)
    {
        var data = Lexer.Instance.ExtractText(token);
        return context
            .RootContext.BitType.Array(BitsConst.BitSize(data[0].GetType()))
            .TextItem
            .Array(data.Length)
            .TextItem
            .Result
                (category, () => CodeBase.BitsConst(BitsConst.ConvertAsText(data)), Closures.Void);
    }
}