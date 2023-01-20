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

    [DisableDump]
//    [EnableDumpExcept(false)]
    public override bool IsProperty
    {
        get {
            if(SourcePart.Id == " MaxNumber8")
                NotImplementedMethod();
            return default;
        }
    }
    [DisableDump]
//    [EnableDumpExcept(false)]
    public override bool IsFunction
    {
        get {
            if(SourcePart.Id == " repeat")
                NotImplementedMethod();
            return default;
        }
    }

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
    public override Issue[] Issues 
        => T(Master.MainAnchor.Issues, Master.Issues)
            .ConcatMany()
            .ToArray();
}