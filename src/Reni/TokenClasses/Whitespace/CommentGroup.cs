using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;

namespace Reni.TokenClasses.Whitespace
{
    class CommentGroup : DumpableObject
    {
        internal interface IConfiguration : LineGroup.IConfiguration
        {
            new int MinimalLineBreakCount { get; }
            new int? EmptyLineLimit { get; }
        }

        [EnableDump]
        readonly SpacesGroup[] Spaces;
        [EnableDump]
        readonly LineGroup[] Lines;
        [EnableDump]
        readonly WhiteSpaceItem Main;

        readonly IConfiguration Configuration;


        CommentGroup(IEnumerable<WhiteSpaceItem> allItems, IConfiguration configuration)
        {
            Configuration = configuration;

            var groups = allItems
                .GroupBy(TailCondition)
                .ToDictionary(item => item.Key, item => item.ToArray());

            groups.TryGetValue(true, out var tails);
            Main = tails?.Single();

            groups.TryGetValue(false, out var items);
            (Lines, Spaces) = LineGroup.Create(items, configuration);
        }

        [DisableDump]
        internal bool IsSeparatorRequired
        {
            get
            {
                if(Main != null)
                    return Main.Type.IsSeparatorRequired;


                NotImplementedMethod();
                return default;
            }
        }

        static bool TailCondition(WhiteSpaceItem item) => item.Type is IComment;

        internal static(CommentGroup[], (LineGroup[], SpacesGroup[])) Create
            (IEnumerable<WhiteSpaceItem> items, IConfiguration configuration)
        {
            var groups = items.SplitAndTail(TailCondition);
            return (groups.Items.Select(items => new CommentGroup(items, configuration)).ToArray()
                , LineGroup.Create(groups.Tail, configuration));
        }

        internal IEnumerable<Edit> GetEdits(int indent, bool isSeparatorRequired)
        {
            if(indent > 0)
            {
                NotImplementedMethod(indent, isSeparatorRequired);
                yield break;
            }

            if(Spaces.Any())
            {
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                NotImplementedMethod(indent, isSeparatorRequired);
                yield break;
            }

            if(Lines.Any())
            {
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                NotImplementedMethod(indent, isSeparatorRequired);
                yield break;
            }

            if(isSeparatorRequired)
            {
                yield return new Edit(Main.SourcePart.Start.Span(0), " ", "+separator");
            }
        }


    }
}