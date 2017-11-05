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
                var newWhiteSpaces = GetWhiteSpaces(item);
                var oldWhiteSpaces = item.WhiteSpaces;

                
                var separator = SeparatorType.Get(LastItem?.TokenClass, item?.TokenClass);

                if(item != null && item.HasWhiteSpaces)
                {
                    NotImplementedMethod(item);
                    return null;
                }

                var add = new List<Edit>();
                if(item != null)
                    if(separator.Text.Length > 0)
                        add.Add
                        (
                            new Edit
                            {
                                NewText = separator.Text,
                                Location = item.Content.Start.Span(0)
                            });

                if(item == null)
                {
                    if(LastItem != null && LastItem.IsEssential)
                        return add;

                    NotImplementedMethod(item);
                    return null;
                }

                if(item.TokenClass == null)
                {
                    if(item.IsEssential)
                    {
                        NotImplementedMethod(item);
                        return null;
                    }


                    NotImplementedMethod(item);
                    return null;
                }

                LastItem = item;
                return add;
            }

            string GetWhiteSpaces(IFormatItem item)
            {
                NotImplementedMethod(item);
                return "";
            }
        }

        readonly Configuration Configuration;
        public SpanFormatter(Configuration configuration) => Configuration = configuration;

        IEnumerable<Edit> IFormatter.GetEditPieces(CompilerBrowser compiler, SourcePart targetPart)
        {
            var worker = new Worker(this);
            return compiler
                .GetTokenList(targetPart)
                .Concat(new IFormatItem []{null})
                .SelectMany(item => worker.Add(item));
        }
    }
}