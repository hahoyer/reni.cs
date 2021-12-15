using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Reni.Parser;

namespace Reni.TokenClasses.Whitespace
{
    class LineGroup : DumpableObject
    {
        internal interface IConfiguration
        {
            int? EmptyLineLimit { get; }
            SeparatorRequests SeparatorRequests { get; }
        }

        [EnableDump]
        internal readonly SourcePart SourcePart;

        [EnableDump]
        internal readonly WhiteSpaceItem Main;

        [EnableDump]
        readonly WhiteSpaceItem[] Spaces;

        internal readonly IConfiguration Configuration;

        internal LineGroup(IEnumerable<WhiteSpaceItem> allItems, IConfiguration configuration)
        {
            Configuration = configuration;
            var groups = allItems
                .GroupBy(TailCondition)
                .ToDictionary(item => item.Key, item => item.ToArray());

            groups.TryGetValue(false, out var items);
            groups.TryGetValue(true, out var tails);

            Main = tails.AssertNotNull().Single();

            Spaces = items ?? new WhiteSpaceItem[0];
            SourcePart = allItems.Select(item => item.SourcePart).Combine();
        }

        internal static(LineGroup[], WhiteSpaceItem[]) Create(WhiteSpaceItem[] items, IConfiguration configuration)
        {
            var groups = items.SplitAndTail(TailCondition);
            return (groups.Items.Select(items => new LineGroup(items, configuration)).ToArray()
                , (groups.Tail));
        }

        internal static bool TailCondition(WhiteSpaceItem item) => item.Type is IVolatileLineBreak;

        internal IEnumerable<Edit> GetEdits()
        {
            if(Spaces.Any())
            {
                NotImplementedMethod();
                return default;
            }

            return new Edit[0];
        }
    }
}