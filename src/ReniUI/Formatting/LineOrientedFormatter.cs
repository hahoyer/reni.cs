using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Reni.Parser;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    public sealed class LineOrientedFormatter : DumpableObject, IFormatter
    {
        internal interface IItem1
        {
            SourcePart Part {get;}
            bool IsRelevant {get;}
            ITokenClass TokenClass {get;}
        }

        internal sealed class Item2
        {
            internal readonly bool IsRelevant;
            internal readonly string NewHeader;
            internal readonly SourcePart Part;

            public Item2(SourcePart part, string newHeader, bool isRelevant)
            {
                Part = part;
                NewHeader = newHeader;
                IsRelevant = isRelevant;
            }
        }

        internal sealed class Line
        {
            static IEnumerable<Item2> AddSeparators(IEnumerable<IItem1> data)
            {
                ITokenClass last = null;
                foreach(var item in data)
                    if(item.IsRelevant)
                    {
                        var current = item.TokenClass;
                        var x = SeparatorExtension.Get(last, current);
                        yield return new Item2(item.Part, x ? " " : "", true);

                        last = current;
                    }
                    else
                        yield return new Item2(item.Part, "", false);
            }

            internal readonly Item2[] Data;

            internal Line(IEnumerable<IItem1> data) => Data = AddSeparators(data).ToArray();

            internal int StartPosition => Data.First().Part.Position;
            internal int EndPosition => Data.Last().Part.EndPosition;
            internal int Lengh => Data.Sum(item => item.Length());
        }

        internal sealed class TokenItem : DumpableObject, IItem1
        {
            readonly Helper.Syntax Target;

            internal TokenItem(Helper.Syntax target) => Target = target;

            SourcePart IItem1.Part => Target.Token.Characters;
            bool IItem1.IsRelevant => Target.TokenClass.Id != "()";

            ITokenClass IItem1.TokenClass => Target.TokenClass;

            string Id => Target.Token.Characters.Id;
            protected override string GetNodeDump() => Id.Quote();
        }

        internal sealed class WhiteSpaceItem : DumpableObject, IItem1
        {
            readonly bool IsRelevant;
            readonly int LineIndex;
            readonly IItem WhiteSpaceToken;

            internal WhiteSpaceItem(int index, int lineIndex, Helper.Syntax target)
            {
                LineIndex = lineIndex;
                WhiteSpaceToken = target.Token.PrecededWith.Skip(index).First();
                IsRelevant = Lexer.IsMultiLineComment(WhiteSpaceToken) || Lexer.IsLineComment(WhiteSpaceToken);
            }

            SourcePart IItem1.Part
            {
                get
                {
                    var part = WhiteSpaceToken.SourcePart;
                    var lineLengths = part.Id.Split('\n').Select(item => item.Length + 1);
                    var start = lineLengths.Take(LineIndex).Sum();
                    return (part.Start + start).Span(Id.Length);
                }
            }

            bool IItem1.IsRelevant => IsRelevant;
            ITokenClass IItem1.TokenClass => null;

            string Id => WhiteSpaceToken.SourcePart.Id.Split('\n')[LineIndex];
            protected override string GetNodeDump() => Id.Quote() + "(=" + LineIndex + ")";
        }

        static IEnumerable<IItem1> GetItems(Helper.Syntax target)
        {
            var whiteSpaceParts = target.Target.Token.PrecededWith.ToArray();
            for(var index = 0; index < whiteSpaceParts.Length; index++)
            {
                var lines = whiteSpaceParts[index].SourcePart.Id.Count(c => c == '\n');

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
        public int? EmptyLineLimit = 1;
        public string IndentItem = "    ";
        public int? MaxLineLength = 100;

        LineOrientedFormatter() {}

        [Obsolete("", true)]
        IEnumerable<Edit> IFormatter.GetEditPieces(CompilerBrowser compiler, SourcePart targetPart)
        {
            var target = compiler.Locate(targetPart);
            var rawLines = target
                    .Chain(item => item.Parent)
                    .Last()
                    .Items
                    .OrderBy(item => item.Target.Token.Characters.Position)
                    .SelectMany(GetItems)
                    .NullableToArray()
                    .Select(CreateLine)
                ;

            while(rawLines.Any(IsTooLongLine))
                rawLines = rawLines.SelectMany(LineBreaker);

            var result = rawLines.Filter(targetPart);

            if(rawLines.Any(IsTooLongLine))
                NotImplementedMethod(compiler, result, targetPart);

            NotImplementedMethod(compiler, target, targetPart);
            return null;
        }

        IEnumerable<Line> LineBreaker(Line arg)
        {
            NotImplementedMethod();
            return null;
        }

        bool IsTooLongLine(Line line)
            => MaxLineLength != null && line.Lengh > MaxLineLength.Value;
    }
}