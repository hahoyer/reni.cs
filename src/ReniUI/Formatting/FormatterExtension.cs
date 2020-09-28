using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
using hw.Scanner;

namespace ReniUI.Formatting
{
    static class FormatterExtension
    {
        public static IFormatter Create(this Configuration configuration)
            => new HierarchicalFormatter(configuration ?? new Configuration());

        internal static string Combine(this IEnumerable<Edit> pieces, SourcePart targetPart)
        {
            pieces = pieces.ToArray();
            var original = targetPart.Id;
            var originalPosition = targetPart.Position;
            var originalEndPosition = originalPosition + original.Length;

            var currentPosition = -originalPosition;
            var result = "";

            var edits = pieces.OrderBy(edit => edit.Location.Position).ToArray();
            foreach(var edit in edits)
            {
                Tracer.Assert(edit.Location.EndPosition <= originalEndPosition, "not implemented.");
                var newPosition = edit.Location.EndPosition - originalPosition;
                if(currentPosition < 0)
                    Tracer.Assert(newPosition <= 0);
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

        public static Helper.Syntax LocateAndFilter(this CompilerBrowser compiler, SourcePart targetPart)
        {
            if(targetPart == null)
                return compiler.Syntax;
            var result = compiler.Locate(targetPart);
            return IsTooSmall(result.Target.Token, targetPart) ? null : result;
        }

        static bool IsTooSmall(IToken token, SourcePart targetPart)
            => token.Characters.Contains
                   (targetPart) ||
               token.PrecededWith.Any(part => part.SourcePart.Contains(targetPart));
    }
}