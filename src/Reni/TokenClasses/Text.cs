using hw.Scanner;
using Reni.Basics;
using Reni.Context;
using Reni.DeclarationOptions;
using Reni.Parser;
using Reni.Type;
using Reni.Validation;

namespace Reni.TokenClasses;

sealed class Text : TerminalSyntaxToken, IIssueTokenClass
{
    static readonly Declaration[] PredefinedDeclarations = { new("dumpprint") };
    readonly IssueId IssueId;

    public Text(IssueId issueId = IssueId.None) => IssueId = issueId;

    IssueId IIssueTokenClass.IssueId => IssueId;

    public override string Id => "<text>";

    protected override Declaration[] Declarations => PredefinedDeclarations;

    protected override Result GetResult(ContextBase context, Category category, SourcePart token)
    {
        var data = Lexer.Instance.ExtractText(token, IssueId == IssueId.None);
        return context
            .RootContext.BitType.GetArray(BitsConst.BitSize(data.FirstOrDefault().GetType()))
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