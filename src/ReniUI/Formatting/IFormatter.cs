using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    public interface IFormatter
    {
        string Reformat(SourceSyntax target, SourcePart part);
    }

    public static class FormatterExtension
    {
        public static IFormatter Create(this Configuration configuration )
            => new HierachicalFormatter(configuration ?? new Configuration());

        public static string Reformat(this SourceSyntax syntax, SourcePart sourcePart, IFormatter formatter = null)
            =>
                (formatter ?? new Configuration().Create()).Reformat
                    (syntax, sourcePart);

        internal static int Length(this LineOrientedFormatter.Item2 item)
        {
            var result = item.NewHeader.Length;
            if(item.IsRelevant)
                result += item.Part.Length;
            return result;
        }

        internal static string Filter
            (this IEnumerable<LineOrientedFormatter.Line> rawLines, SourcePart part)
        {
            var result = "";
            var lines = rawLines
                .Select(line => line.Data.Where(item => item.Contains(part)))
                .SelectMany(items => items.Where(item => item.IsRelevant));

            foreach(var item in lines)
                result += item.NewValue(part);
            return result;
        }

        static string NewValue(this LineOrientedFormatter.Item2 item, SourcePart part)
        {
            var subResult = "";
            if(item.Part.Position >= part.Position)
                subResult += item.NewHeader;
            subResult += part.Intersect(item.Part).Id;
            return subResult;
        }

        internal static bool Contains(this LineOrientedFormatter.Item2 item, SourcePart part)
            => item.Part.Position < part.EndPosition
                && item.Part.EndPosition > part.Position;
    }
}