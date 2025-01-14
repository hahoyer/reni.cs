using hw.Scanner;
using Reni.TokenClasses.Whitespace.Comment;

namespace Reni.TokenClasses.Whitespace;

sealed class LineGroup : DumpableObject
{
    internal interface IConfiguration
    {
        int? EmptyLineLimit { get; }
        SeparatorRequests SeparatorRequests { get; }
        int MinimalLineBreakCount { get; }
        string LineBreakString { get; }
    }

    [EnableDump]
    readonly SourcePart SourcePart;

    readonly IItemType? Predecessor;
    readonly bool IsLast;
    readonly IConfiguration Configuration;

    int PredecessorLine => Predecessor is ILine? 1 : 0;

    /// <summary>
    ///     When this line group belongs to a comment group it is 0.<br />
    ///     Otherwise, it is the minimal line break count from configuration,<br />
    ///     reduced by one if the predecessor is a line comment.<br />
    /// </summary>
    /// <remarks>
    ///     Can be -1 if <br />
    ///     it is last,<br />
    ///     predecessor is a line comment and<br />
    ///     minimal line break count by configuration is 0.
    /// </remarks>
    int MinimalLineBreakCount => IsLast? Configuration.MinimalLineBreakCount - PredecessorLine : 0;

    /// <summary>
    ///     When there is no line limit by configuration it is the number of lines in this group.<br />
    ///     Otherwise, it is the minimum of the line limit by configuration<br />
    ///     - reduced by one if the predecessor is a line comment - and number of lines.
    /// </summary>
    /// <remarks>
    ///     Can be -1 if <br />
    ///     predecessor is a line comment and <br />
    ///     configuration line limit is 0.
    /// </remarks>
    int LineBreaksToKeep => Configuration.EmptyLineLimit == null
        ? field
        : T(Configuration.EmptyLineLimit.Value - PredecessorLine, field).Min();

    /// <summary>
    ///     Target line count respects everything:<br />
    ///     the current number of lines (including any preceding line comment),<br />
    ///     the empty line limit of configuration if set and<br />
    ///     if it is the last lines-and-spaces-group, the minimal line break count of configuration
    /// </summary>
    int TargetLineCount => T(MinimalLineBreakCount, LineBreaksToKeep, 0).Max();

    /// <summary>
    ///     Line break mode is when predecessor is line comment or <see cref="TargetLineCount" /> is positive
    /// </summary>
    bool LineBreakMode => Predecessor is ILine || TargetLineCount > 0;

    /// <summary>
    ///     Will be null if it is line break mode.<br />
    ///     Otherwise, it will be zero or one spaces<br />
    ///     depending on separator request of the current position (head/inner/tail/flat).
    /// </summary>
    int? TargetSpacesCount => LineBreakMode? null :
        Configuration.SeparatorRequests.Get(Predecessor == null, IsLast)? 1 : 0;

    internal LineGroup
    (
        SourcePart sourcePart
        , IItemType? predecessor
        , int lines
        , bool isLast
        , IConfiguration configuration
    )
    {
        SourcePart = sourcePart;
        Predecessor = predecessor;
        LineBreaksToKeep = lines;
        IsLast = isLast;
        Configuration = configuration;
    }

    protected override string GetNodeDump() => SourcePart.NodeDump + " " + base.GetNodeDump();

    internal IEnumerable<Edit> GetEdits(int indent)
    {
        var insert = Configuration.LineBreakString.Repeat(TargetLineCount) + " ".Repeat(TargetSpacesCount ?? indent);
        return Edit.Create(SourcePart, insert);
    }
}