using hw.Scanner;
using Reni.Basics;
using Reni.Context;
using Reni.DeclarationOptions;
using Reni.Parser;
using Reni.Type;

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
                (category, () => Code.Extension.GetCode(BitsConst.ConvertAsText(data)), Closures.GetVoid);
    }

    protected override TypeBase TryGetTypeBase(SourcePart token)
    {
        NotImplementedMethod(token);
        return default;
    }
}