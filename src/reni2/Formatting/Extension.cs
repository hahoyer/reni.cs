using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Helper;
using hw.Parser;
using hw.Scanner;
using Reni.TokenClasses;

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
                    yield return new Item(null, "\n");


                var isMultiline =
                    line.SelectMany(item => item.WhiteSpaces.Where(c => c == '\n')).Any();

                if(addNewLine && (wasMultiline || isMultiline))
                    yield return new Item(null, "\n");

                foreach(var item in line)
                    yield return item;

                wasMultiline = isMultiline;
                addNewLine = true;
            }
        }

        internal static IEnumerable<Item> Combine
            (this IEnumerable<Item> items)
        {
            var itemPart = "";
            foreach(var item in items)
            {
                itemPart += item.WhiteSpaces;
                if(item.Token != null)
                {
                    yield return new Item(item.Token, itemPart);
                    itemPart = "";
                }
            }

            Tracer.Assert(itemPart == "");
        }

        internal static bool IsChain(this SourceSyntax target)
        {
            if(target?.Left == null)
                return false;

            var tokenClass = target.TokenClass;

            if(tokenClass is ReassignToken || tokenClass is Colon || tokenClass is Function
                || tokenClass is RightParenthesis || tokenClass is List)
                return false;

            if(tokenClass is Definable || tokenClass is InstanceToken)
                return true;

            Dumpable.NotImplementedFunction(target);
            return false;
        }

        internal static string Filter
            (this IEnumerable<Item> items, SourcePart targetPart)
        {
            return items
                .Select(item => item.Filter(targetPart))
                .Stringify("");
        }
    }
}