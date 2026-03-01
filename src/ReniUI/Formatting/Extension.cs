#nullable enable
using hw.Scanner;

namespace ReniUI.Formatting;

static class Extension
{
    [UsedImplicitly]
    static TValue[] T<TValue>(params TValue[] value) => value;

    extension(Configuration? configuration)
    {
        internal IFormatter Formatter => new FormatterByBinaryTree(configuration ?? new Configuration());
    }

    extension(IEnumerable<Edit> pieces)
    {
        internal string Combine(SourcePart targetPart)
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
    }
}