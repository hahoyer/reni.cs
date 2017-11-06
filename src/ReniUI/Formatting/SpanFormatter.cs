using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    public class SpanFormatter : DumpableObject, IFormatter
    {
        class Worker : DumpableObject
        {
            readonly SpanFormatter Parent;

            [EnableDump]
            IFormatItem LastItem;

            public Worker(SpanFormatter parent) => Parent = parent;

            public IEnumerable<Edit> Add(IFormatItem item)
            {
                var result = ToEdits(item, GetWhiteSpaces(item));
                LastItem = item;
                return result;
            }

            IEnumerable<Edit> ToEdits(IFormatItem item, string newWhiteSpaces)
            {
                if(newWhiteSpaces == (item?.WhiteSpaces ?? ""))
                    return new Edit[0];

                if(item == null || item.WhiteSpaces == "")
                {
                    var location = (item?.Content.Start ?? LastItem.Content.End).Span(0);
                    return new[]
                    {
                        new Edit
                        {
                            NewText = newWhiteSpaces,
                            Location = location
                        }
                    };
                }

                NotImplementedMethod(item, newWhiteSpaces);
                return null;
            }

            string GetWhiteSpaces(IFormatItem item)
            {
                if(item == null)
                    return GetWhiteSpacesAtEndOfRegion();

                if(LastItem == null)
                    return GetWhiteSpacesAtStartOfRegion(item);

                if(LastItem.TokenClass == null)
                    return GetWhiteSpacesAtStartOfRegionAfterWhiteSpaces(item);

                if(item.HasEssentialWhiteSpaces)
                {
                    NotImplementedMethod(item);
                    return "";
                }

                var separator = SeparatorType.Get(LastItem.TokenClass, item.TokenClass);
                return separator.Text;
            }

            string GetWhiteSpacesAtEndOfRegion() => "";

            string GetWhiteSpacesAtStartOfRegionAfterWhiteSpaces(IFormatItem item)
            {
                NotImplementedMethod(item);
                return null;
            }

            string GetWhiteSpacesAtStartOfRegion(IFormatItem item) => "";
        }

        readonly Configuration Configuration;
        public SpanFormatter(Configuration configuration) => Configuration = configuration;

        IEnumerable<Edit> IFormatter.GetEditPieces(CompilerBrowser compiler, SourcePart targetPart)
        {
            var worker = new Worker(this);
            var items = compiler
                .GetTokenList(targetPart)
                .Concat(new IFormatItem[] {null});
            return items
                .SelectMany(item => worker.Add(item));
        }
    }
}