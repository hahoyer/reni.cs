using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;
using Reni.TokenClasses.Whitespace.Comment;

namespace Reni.TokenClasses.Whitespace;

sealed class CommentGroup : DumpableObject
{
    sealed class CommentConfiguration : DumpableObject, LinesAndSpaces.IConfiguration
    {
        readonly CommentGroup Prefix;
        readonly LineGroup.IConfiguration Configuration;

        internal CommentConfiguration(LineGroup.IConfiguration configuration, CommentGroup prefix)
        {
            Configuration = configuration;
            Prefix = prefix;
        }

        int? LineGroup.IConfiguration.EmptyLineLimit => Configuration.EmptyLineLimit;
        bool LinesAndSpaces.IConfiguration.IsSeparatorRequired => true;
        int LineGroup.IConfiguration.MinimalLineBreakCount => 0;
        CommentGroup LinesAndSpaces.IConfiguration.Prefix => Prefix;
        SeparatorRequests LineGroup.IConfiguration.SeparatorRequests => Configuration.SeparatorRequests;
    }

    sealed class LinesAndSpacesConfiguration : DumpableObject, LinesAndSpaces.IConfiguration
    {
        [EnableDump]
        readonly LineGroup.IConfiguration Configuration;

        [EnableDump]
        readonly CommentGroup Prefix;

        [EnableDump]
        readonly bool IsForTail;

        internal LinesAndSpacesConfiguration
            (LineGroup.IConfiguration configuration, CommentGroup prefix, bool isForTail = false)
        {
            Configuration = configuration;
            Prefix = prefix;
            IsForTail = isForTail;
            StopByObjectIds();
        }

        int? LineGroup.IConfiguration.EmptyLineLimit => Configuration.EmptyLineLimit;

        bool LinesAndSpaces.IConfiguration.IsSeparatorRequired
        {
            get
            {
                if(IsForTail)
                    return Configuration.MinimalLineBreakCount == 0 &&
                    (
                        Prefix == null
                            ? Configuration.SeparatorRequests.Flat
                            : Configuration.SeparatorRequests.Tail && Prefix.IsSeparatorRequired
                    );

                return Prefix == null
                    ? Configuration.SeparatorRequests.Head
                    : Configuration.SeparatorRequests.Inner && Prefix.Main.Type is IInline;
            }
        }

        int LineGroup.IConfiguration.MinimalLineBreakCount => Configuration.MinimalLineBreakCount;

        CommentGroup LinesAndSpaces.IConfiguration.Prefix => Prefix;
        SeparatorRequests LineGroup.IConfiguration.SeparatorRequests => Configuration.SeparatorRequests;
    }

    [EnableDump]
    internal readonly WhiteSpaceItem Main;

    [EnableDump]
    readonly LinesAndSpaces LinesAndSpaces;

    CommentGroup
    (
        IEnumerable<WhiteSpaceItem> allItems
        , LinesAndSpaces.IConfiguration configuration
    )
    {
        var groups = allItems
            .GroupBy(TailCondition)
            .ToDictionary(item => item.Key, item => item.ToArray());

        Main = groups[true].Single();

        groups.TryGetValue(false, out var items);
        LinesAndSpaces = items == null || !items.Any()
            ? LinesAndSpaces.Create(Main.SourcePart.Start, configuration)
            : LinesAndSpaces.Create(items, configuration);
    }

    [DisableDump]
    internal bool IsSeparatorRequired => Main.Type.IsSeparatorRequired;

    static bool TailCondition(WhiteSpaceItem item) => item.Type is IComment;

    internal static(CommentGroup[], LinesAndSpaces) Create
        (IEnumerable<WhiteSpaceItem> allItems, LineGroup.IConfiguration configuration)
    {
        var groups = allItems.SplitAndTail(TailCondition);

        var commentGroups = new List<CommentGroup>();
        CommentGroup commentGroup = default;
        foreach(var items in groups.Items)
        {
            var commentConfiguration = new CommentConfiguration(configuration, commentGroup);
            commentGroup = new(items, new LinesAndSpacesConfiguration(commentConfiguration, commentGroup));
            commentGroups.Add(commentGroup);
        }

        var linesAndSpacesConfiguration
            = new LinesAndSpacesConfiguration(configuration, commentGroup, true);
        var linesAndSpaces =
                groups.Tail.Any()
                    ? LinesAndSpaces.Create(groups.Tail, linesAndSpacesConfiguration)
                    : LinesAndSpaces.Create(allItems.Last().SourcePart.End, linesAndSpacesConfiguration)
            ;

        return (commentGroups.ToArray(), linesAndSpaces);
    }

    internal static(CommentGroup[] Comments, LinesAndSpaces LinesAndSpaces) Create
        (SourcePosition anchor, LineGroup.IConfiguration configuration)
    {
        var linesAndSpacesConfiguration
            = new LinesAndSpacesConfiguration(configuration, null, true);
        return (new CommentGroup[0], LinesAndSpaces.Create(anchor, linesAndSpacesConfiguration));
    }

    internal IEnumerable<Edit> GetEdits(int indent) => LinesAndSpaces.GetEdits(indent);
}