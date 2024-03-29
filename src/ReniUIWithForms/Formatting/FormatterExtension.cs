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

            var edits = pieces
                .OrderBy(edit => edit.Location.Position)
                .Where(edit => edit.Location.Intersect(targetPart) != null)
                .ToArray();

            foreach(var edit in edits)
            {
                (edit.Location.EndPosition <= originalEndPosition).Assert("not implemented.");
                var newPosition = edit.Location.EndPosition - originalPosition;
                if(currentPosition < 0)
                    (newPosition <= 0).Assert();
                else
                {
                    var length = edit.Location.Position - originalPosition - currentPosition;
                    var itemResult = original.Substring(currentPosition, length) + edit.NewText;
                    result += itemResult;
                }

                currentPosition = newPosition;
            }

            result += original.Substring(T(0, currentPosition).Max());
            return result;
        }

        public static bool IsTooSmall(this CompilerBrowser compiler, SourcePart targetPart)
        {
            if(targetPart == null)
                return false;

            var start = compiler.LocatePosition(targetPart.Start);
            var end = compiler.LocatePosition(targetPart.End+- 1);
            if(start != null && end != null)
                return start.Master == end.Master && IsTooSmall(start.Token, targetPart);

            Dumpable.NotImplementedFunction(compiler, targetPart);
            return default;
        }

        static bool IsTooSmall(IToken token, SourcePart targetPart)
            => token.Characters.Contains
                   (targetPart) ||
               token.PrecededWith.Any(part => part.SourcePart.Contains(targetPart));

        public static TValue[] T<TValue>(params TValue[] value) => value;
    }
}