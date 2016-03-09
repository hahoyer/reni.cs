using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using hw.Scanner;
using Reni.Parser;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    public sealed class LineOrientedFormatter : DumpableObject, IFormatter
    {
        public int? MaxLineLength = 100;
        public int? EmptyLineLimit = 1;
        public string IndentItem = "    ";

        LineOrientedFormatter() { }

        string IFormatter.Reformat(SourceSyntax target, SourcePart part)
        {
            var rawLines = target
                .Chain(item => item.Parent)
                .Last()
                .Items
                .OrderBy(item => item.Token.SourcePart.Position)
                .SelectMany(GetItems)
                .NullableToArray()
                .Select(CreateLine)
                ;

            while(rawLines.Any(IsTooLongLine))
                rawLines = rawLines.SelectMany(LineBreaker);

            var result = rawLines.Filter(part);

            if(rawLines.Any(IsTooLongLine))
                NotImplementedMethod(result, part);

            return result;
        }

        IEnumerable<EditPiece> IFormatter.GetEditPieces(SourceSyntax target, SourcePart targetPart)
        {
            NotImplementedMethod(target, targetPart);
            return null;
        }

        IEnumerable<Line> LineBreaker(Line arg)
        {
            NotImplementedMethod();
            return null;
        }

        bool IsTooLongLine(Line line)
            => MaxLineLength != null && line.Lengh > MaxLineLength.Value;

        static IEnumerable<IItem1> GetItems
            (SourceSyntax target)
        {
            for(var index = 0; index < target.Token.PrecededWith.Length; index++)
            {
                var lines = target
                    .Token
                    .PrecededWith[index]
                    .Characters
                    .Id
                    .Count(c => c == '\n');

                for(var lineIndex = 0; lineIndex < lines; lineIndex++)
                {
                    IItem1 item = new WhiteSpaceItem(index, lineIndex, target);
                    yield return item;
                }
                yield return new WhiteSpaceItem(index, lines, target);
            }
            yield return new TokenItem(target);
        }

        static Line CreateLine(IEnumerable<IItem1> item) => new Line(item);

        internal interface IItem1
        {
            SourcePart Part { get; }
            bool IsRelevant { get; }
            ITokenClass TokenClass { get; }
        }

        internal sealed class Item2
        {
            internal readonly SourcePart Part;
            internal readonly string NewHeader;
            internal readonly bool IsRelevant;

            public Item2(SourcePart part, string newHeader, bool isRelevant)
            {
                Part = part;
                NewHeader = newHeader;
                IsRelevant = isRelevant;
            }
        }

        internal sealed class Line
        {
            internal readonly Item2[] Data;

            internal Line(IEnumerable<IItem1> data) { Data = AddSepearators(data).ToArray(); }

            static IEnumerable<Item2> AddSepearators(IEnumerable<IItem1> data)
            {
                ITokenClass last = null;
                foreach(var item in data)
                    if(item.IsRelevant)
                    {
                        var current = item.TokenClass;
                        var x = SeparatorType.Get(last, current);
                        yield return new Item2(item.Part, x.Text, true);

                        last = current;
                    }
                    else
                        yield return new Item2(item.Part, "", false);
            }

            internal int StartPosition => Data.First().Part.Position;
            internal int EndPosition => Data.Last().Part.EndPosition;
            internal int Lengh => Data.Sum(item => item.Length());
        }

        internal sealed class TokenItem : DumpableObject, IItem1
        {
            readonly SourceSyntax Target;

            internal TokenItem(SourceSyntax target) { Target = target; }

            string Id => Target.Token.Characters.Id;
            protected override string GetNodeDump() => Id.Quote();

            SourcePart IItem1.Part => Target.Token.Characters;
            bool IItem1.IsRelevant => Target.TokenClass.Id != "()";

            ITokenClass IItem1.TokenClass => Target.TokenClass;
        }

        internal sealed class WhiteSpaceItem : DumpableObject, IItem1
        {
            readonly int LineIndex;
            readonly WhiteSpaceToken WhiteSpaceToken;
            readonly bool IsRelevant;

            internal WhiteSpaceItem(int index, int lineIndex, SourceSyntax target)
            {
                LineIndex = lineIndex;
                WhiteSpaceToken = target.Token.PrecededWith[index];
                IsRelevant = Lexer.IsComment(WhiteSpaceToken)
                    || Lexer.IsLineComment(WhiteSpaceToken);
            }

            string Id => WhiteSpaceToken.Characters.Id.Split('\n')[LineIndex];
            protected override string GetNodeDump() => Id.Quote() + "(=" + LineIndex + ")";

            SourcePart IItem1.Part
            {
                get
                {
                    var part = WhiteSpaceToken.Characters;
                    var lineLengths = part.Id.Split('\n').Select(item => item.Length + 1);
                    var start = lineLengths.Take(LineIndex).Sum();
                    return (part.Start + start).Span(Id.Length);
                }
            }

            bool IItem1.IsRelevant => IsRelevant;
            ITokenClass IItem1.TokenClass => null;
        }
    }
}