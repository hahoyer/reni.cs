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

    protected override Result GetResult(ContextBase context, Category category, SourcePart token)
    {
        var data = Lexer.Instance.ExtractText(token);
        return context
            .RootContext.BitType.GetArray(BitsConst.BitSize(data[0].GetType()))
            .TextItem
            .GetArray(data.Length)
            .TextItem
            .GetResult
                (category, () => CodeBase.GetBitsConst(BitsConst.ConvertAsText(data)), Closures.Void);
    }
}