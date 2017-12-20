using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
using hw.Scanner;
using Reni.TokenClasses;


namespace ReniUI.Formatting
{
    public static class FormatterExtension
    {
        public static IFormatter Create(this Configuration configuration)
            => new StructFormatter(configuration ?? new Configuration());

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
                .Select(line => line.Data.Where(item => Contains(item, part)))
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

        internal static string Combine(this IEnumerable<Edit> pieces, CompilerBrowser compiler, SourcePart targetPart)
        {
            var original = targetPart?.Id ?? compiler.Source.Data;
            var originalPosition = targetPart?.Position ?? 0;
            var originalEndPosition = originalPosition + original.Length;

            var currentPosition = -originalPosition;
            var result = "";

            foreach(var edit in pieces.OrderBy(edit => edit.Location.Position))
            {
                Tracer.Assert(edit.Location.EndPosition <= originalEndPosition,"not implemented.");
                var newPosition = edit.Location.EndPosition - originalPosition;
                if (currentPosition < 0)
                {
                    Tracer.Assert(newPosition <= 0);
                }
                else
                {
                    var length = edit.Location.Position - originalPosition - currentPosition;
                    var itemResult = original.Substring(currentPosition, length) + edit.NewText;
                    result += itemResult;
                }

                currentPosition = newPosition;
            }

            result += original.Substring(Math.Max(0, currentPosition));
            return result;
        }

        public static Syntax LocateAndFilter(this CompilerBrowser compiler, SourcePart targetPart)
        {
            var result = compiler.Locate(targetPart);
            return IsTooSmall(result.Token, targetPart) ? null : result;
        }

        static bool IsTooSmall(IToken resultToken, SourcePart targetPart)
        {
            var sourcePart = resultToken.SourcePart();

            if (targetPart.End > sourcePart.End)
                return false;

            if (targetPart.Start < sourcePart.Start)
                return false;

            if (!resultToken.PrecededWith.Any())
                return true;

            if (targetPart.Start >= resultToken.Characters.Start)
                return true;

            if(targetPart.End > resultToken.Characters.Start)
                return false;

            foreach(var item in resultToken.PrecededWith)
            {
                var part = item.SourcePart;
                if(part.Contains(targetPart))
                    return true;
                var intersect = part.Intersect(targetPart);
                if(intersect != null && intersect.Length > 0)
                    return false;
            }

            DumpableObject.NotImplementedFunction(resultToken, targetPart);
            return false;
        }
    }
}