using System.Diagnostics;
using hw.Scanner;
using Reni.Helper;
using Reni.TokenClasses;
using Reni.TokenClasses.Brackets;

namespace ReniUI.Formatting;

sealed class BinaryTreeProxy
    : TreeWithParentExtended<BinaryTreeProxy, BinaryTree>
        , ValueCache.IContainer
        , BinaryTree.IPositionTarget
{
    [DisableDump]
    public readonly Configuration Configuration;

    [EnableDump(Order = 2)]
    [EnableDumpExcept(null)]
    internal Position LineBreakBehaviour;

    [EnableDump(Order = -3)]
    string MainPosition
        => FlatItem.SourcePart.GetDumpAroundCurrent()
            + " "
            + FlatItem.TokenClass.GetType().PrettyName()
            + (FlatItem.Formatter == default? "" : " " + FlatItem.Formatter.GetType().Name);

    [EnableDump(Order = 3)]
    [EnableDumpExcept(false)]
    internal bool IsLineSplit => GetIsLineSplit();

    [EnableDump(Order = 3.1)]
    [EnableDumpExcept(false)]
    internal bool IsLineSplitRight
    {
        get
        {
            if(!IsLineSplit)
                return false;

            if(Right == null)
                return false;

            if(Right.FlatItem == null)
            {
                IsInDump.Assert();
                return false;
            }

            var flatLength = Right.FlatLength;
            if(flatLength == null)
                return true;

            var maxLength = Configuration.MaxLineLength;
            if(maxLength == null)
                return false;
            if(FlatItem.Token.Length + flatLength > maxLength.Value)
                return true;
            return false;
        }
    }

    [EnableDump(Order = 3.1)]
    [EnableDumpExcept(false)]
    bool ForceLineSplit => LineBreakBehaviour != null && LineBreakBehaviour.ForceLineBreak != default;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    bool HasAlreadyLineBreakOrIsTooLong => GetHasAlreadyLineBreakOrIsTooLong();

    [DisableDump]
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    internal ISourcePartEdit[] Edits => GetEdits();

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    ISourcePartEdit[] ChildrenEdits
    {
        get
        {
            StopByObjectIds();
            return DirectChildren
                .Where(item => item != null)
                .SelectMany(item => item.Edits)
                .ToArray();
        }
    }

    [EnableDump(Order = 4)]
    [EnableDumpExcept(null)]
    internal BinaryTreeProxy Left => DirectChildren[0];

    [EnableDump(Order = 5)]
    [EnableDumpExcept(null)]
    internal BinaryTreeProxy Right => DirectChildren[1];

    ISourcePartEdit[] AnchorEdits
        => WhiteSpaces
            .Create(FlatItem, Configuration, LineBreakCount, Token.LogDump())
            .ToArray();

    SourcePart Token => FlatItem.FullToken;

    int LineBreakCount => (int)(LineBreakBehaviour?.LineBreaks ?? Position.Flag.LineBreaks.False);

    int? FlatLength => FlatItem.GetFlatLength(Configuration.EmptyLineLimit != 0);

    bool HasLineBreaksBySyntax
        => GetHasLineBreaksBySyntax()
            || (Left != null && Left.HasLineBreaksBySyntax)
            || (Right != null && Right.HasLineBreaksBySyntax);

    BinaryTreeProxy(BinaryTree target, Configuration configuration, BinaryTreeProxy parent)
        : base(target, parent)
    {
        Configuration = configuration;
        Formatter.SetFormatters(target);
        StopByObjectIds();
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    ValueCache ValueCache.IContainer.Cache { get; } = new();

    protected override string Dump(bool isRecursion) => FlatItem == null? "?" : base.Dump(isRecursion);

    protected override BinaryTreeProxy Create(BinaryTree child)
        => child == null? null : new(child, Configuration, this);

    bool GetIsLineSplit()
    {
        if(FlatItem.TokenClass is ILeftBracket)
            return Parent!.IsLineSplit;
        return HasAlreadyLineBreakOrIsTooLong || ForceLineSplit || HasLineBreaksBySyntax;
    }

    bool GetHasLineBreaksBySyntax()
        => Configuration.LineBreaksAtComplexDeclaration && FlatItem.HasComplexDeclaration;

    bool GetHasAlreadyLineBreakOrIsTooLong()
    {
        if(FlatItem == null)
            return false;
        var lineLength = FlatLength;
        return lineLength == null || lineLength > Configuration.MaxLineLength;
    }

    internal void SetPosition(Position position)
        => LineBreakBehaviour += position;

    internal void SetupPositions()
    {
        if(!IsLineSplit)
            return;

        SetupMainPositions();
        Left?.SetupPositions();
        Right?.SetupPositions();
    }

    internal BinaryTreeProxy Convert(BinaryTree target)
        => FlatItem == target? this :
            target.FullToken.End <= FlatItem.FullToken.Start? Left?.Convert(target) : Right?.Convert(target);

    void SetupMainPositions() => FlatItem.Formatter?.SetupPositions(this);

    internal static BinaryTreeProxy Create(BinaryTree target, Configuration configuration)
        => new(target, configuration, null);

    ISourcePartEdit[] GetEdits()
        => T(AnchorEdits, ChildrenEdits)
            .ConcatMany()
            .Indent(LineBreakBehaviour == null || LineBreakBehaviour.Indent == default? 0 : 1)
            .ToArray();
}