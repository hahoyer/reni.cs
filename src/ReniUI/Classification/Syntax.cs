using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Reni;
using Reni.TokenClasses;
using Reni.Validation;

namespace ReniUI.Classification;

sealed class Syntax : Item
{
    internal Syntax(BinaryTree anchor)
        : base(anchor) { }

    [EnableDumpExcept(false)]
    public override bool IsKeyword => !IsIdentifier && !IsNumber && !IsText && !IsBrace;

    [EnableDumpExcept(false)]
    public override bool IsIdentifier => TokenClass is Definable;

    [EnableDumpExcept(false)]
    public override bool IsText => TokenClass is Text;

    [EnableDumpExcept(false)]
    public override bool IsNumber => TokenClass is Number;

    [EnableDumpExcept(false)]
    public override bool IsError => Issues != null && Issues.Any();

    [EnableDumpExcept(false)]
    public override bool IsBraceLike => TokenClass is IBelongingsMatcher;

    [EnableDumpExcept(false)]
    public override bool IsBrace
        => TokenClass is IBracket;

    [EnableDumpExcept(false)]
    public override bool IsPunctuation
        => TokenClass is List;

    [DisableDump]
    public override string State => Anchor.Token.Id ?? "";

    public override IEnumerable<SourcePart> ParserLevelGroup
        => Master
            .Anchor
            .Items
            .Where(node => Anchor.TokenClass.IsBelongingTo(node.TokenClass))
            .Select(node => node.Token);

    [DisableDump]
    public override Issue[] Issues => T(T(Master.MainAnchor.Issue), Master.Issues).ConcatMany().ToArray();
}