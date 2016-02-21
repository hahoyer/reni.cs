using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;

namespace Reni.Formatting
{
    static class Extension
    {
        internal static IEnumerable<Item> PrettyLines
            (this IEnumerable<IEnumerable<Item>> lines)
        {
            var wasMultiline = false;
            var addNewLine = false;

            foreach(var line in lines)
            {
                if(addNewLine)
                    yield return new Item("\n");

                var isMultiline =
                    line
                        .SelectMany(item => item.WhiteSpaces.Where(c => c == '\n'))
                        .Any();

                if(addNewLine && (wasMultiline || isMultiline))
                    yield return new Item("\n");

                foreach(var item in line)
                    yield return item;

                wasMultiline = isMultiline;
                addNewLine = true;
            }
        }

        internal static IEnumerable<Item> Combine(this IEnumerable<Item> items)
        {
            var itemPart = "";
            foreach(var item in items)
            {
                itemPart += item.WhiteSpaces;
                if(item.Token != null)
                {
                    yield return new Item(itemPart, item.Token);
                    itemPart = "";
                }
            }

            Tracer.Assert(itemPart == "");
        }
    }
}