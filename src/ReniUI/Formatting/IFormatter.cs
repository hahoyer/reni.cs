using System.Collections.Generic;
using System.Linq;
using hw.Scanner;

namespace ReniUI.Formatting
{
    public interface IFormatter
    {
        IEnumerable<Edit> GetEditPieces(CompilerBrowser compiler, SourcePart targetPart);
    }

    public static class FormatterExtension
    {
        public static IFormatter Create(this Configuration configuration)
            => new SpanFormatter(configuration ?? new Configuration());

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
            => item.Part.Position < part.EndPosition && item.Part.EndPosition > part.Position;

        internal static string Combine(this IEnumerable<Edit> pieces, CompilerBrowser compiler)
        {
            var original = compiler.Source.Data;
            var current = 0;
            var result = "";

            foreach(var edit in pieces.OrderBy(edit => edit.Location.Position))
            {
                var length = edit.Location.Position - current;
                result += original.Substring(current, length);
                result += edit.NewText;
                current += length;
            }

            result += original.Substring(current);
            return result;
        }
    }
}