using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;

namespace ReniUI.Formatting
{
    static class FormatterExtension
    {
        public static IFormatter Create(this Configuration configuration)
            => new FormatterByBinaryTree(configuration ?? new Configuration());

        internal static string Combine(this IEnumerable<Edit> pieces, SourcePart targetPart)
        {
            pieces = pieces.ToArray();
            var original = targetPart.Id;
            var originalPosition = targetPart.Position;
            var originalEndPosition = originalPosition + original.Length;

            var currentPosition = -originalPosition;
            var result = "";

            var edits = pieces
                .OrderBy(edit => edit.Remove.Position)
                .Where(edit => edit.Remove.Intersect(targetPart) != null)
                .ToArray();

            foreach(var edit in edits)
            {
                (edit.Remove.EndPosition <= originalEndPosition).Assert("not implemented.");
                var newPosition = edit.Remove.EndPosition - originalPosition;
                if(currentPosition < 0)
                    (newPosition <= 0).Assert();
                else
                {
                    var length = edit.Remove.Position - originalPosition - currentPosition;
                    var itemResult = original.Substring(currentPosition, length) + edit.Insert;
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

            var start = compiler.Locate(targetPart.Start);
            var end = compiler.Locate(targetPart.End+- 1);
            if(start != null && end != null)
                return start.Anchor == end.Anchor && IsTooSmall(start.SourcePart, targetPart);

            Dumpable.NotImplementedFunction(compiler, targetPart);
            return default;
        }

        static bool IsTooSmall(SourcePart fullToken, SourcePart targetPart)
            => fullToken.Contains(targetPart);

        public static TValue[] T<TValue>(params TValue[] value) => value;
    }
}