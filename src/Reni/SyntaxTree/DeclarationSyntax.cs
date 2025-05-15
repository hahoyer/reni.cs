using hw.Scanner;
using Reni.Validation;

namespace Reni.SyntaxTree;

sealed class DeclarationSyntax : Syntax, IStatementSyntax
{
    [EnableDumpExcept(null)]
    internal readonly DeclarerSyntax? Declarer;

    [EnableDumpExcept(null)]
    internal readonly ValueSyntax Value;

    [DisableDump]
    internal string? NameOrNull => Declarer?.Name?.Value;

    [DisableDump]
    internal bool IsConverterSyntax => Declarer?.IsConverterSyntax ?? false;

    [DisableDump]
    internal bool IsMutableSyntax => Declarer?.IsMutableSyntax ?? false;

    int DirectChildCountOfDeclarer => Declarer?.DirectChildCount ?? 0;

    DeclarationSyntax(DeclarerSyntax? declarer, ValueSyntax value, Anchor anchor)
        : base(anchor)
    {
        Declarer = declarer;
        Value = value;
    }

    [DisableDump]
    DeclarerSyntax? IStatementSyntax.Declarer => Declarer;

    SourcePart IStatementSyntax.SourcePart => Anchor.SourcePart;

    ValueSyntax IStatementSyntax.ToValueSyntax(Anchor anchor)
        => CompoundSyntax.Create([this], null, anchor);

    [DisableDump]
    ValueSyntax IStatementSyntax.Value => Value;

    IStatementSyntax IStatementSyntax.With(Anchor? anchor)
        => anchor == null || !anchor.Items.Any()
            ? this
            : Create(Declarer, Value, anchor.Combine(Anchor, true));

    protected override IEnumerable<Issue> GetIssues()
    {
        if(Declarer == null)
            yield return IssueId.MissingDeclarationDeclarer.GetIssue(Anchor.Main.Root, Anchor.Main.SourcePart);
    }

    [DisableDump]
    protected override int DirectChildCount => DirectChildCountOfDeclarer + 1;

    protected override Syntax? GetDirectChild(int index)
        => index switch
        {
            { } when index < DirectChildCountOfDeclarer => Declarer!.GetDirectChild(index)
            , { } when index == DirectChildCountOfDeclarer => Value
            , var _ => null
        };

    internal static IStatementSyntax Create(DeclarerSyntax? declarer, ValueSyntax value, Anchor anchor)
        => new DeclarationSyntax(declarer, value, anchor);
}