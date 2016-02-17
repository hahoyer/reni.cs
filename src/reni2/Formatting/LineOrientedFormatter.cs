using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using hw.Scanner;
using Reni.TokenClasses;

namespace Reni.Formatting
{
    public sealed class LineOrientedFormatter : DumpableObject, IFormatter
    {
        interface IItem
        {
        }

        string IFormatter.Reformat(SourceSyntax target, SourcePart part)
        {
            var lines = target
                .Chain(item=>item.Parent)
                .Last()
                .Items
                .OrderBy(item=>item.Token.SourcePart.Position)
                .SelectMany(GetItems)
                .Split(item => item == null)
                .ToArray();


            NotImplementedMethod(target, part);
            return null;
        }

        static IEnumerable<IItem> GetItems(SourceSyntax target)
        {
            for(var index = 0; index < target.Token.PrecededWith.Length; index++)
            {
                var lines = target
                    .Token
                    .PrecededWith[index]
                    .Characters
                    .Id
                    .Count(c=>c=='\n');

                for (var lineIndex = 0; lineIndex < lines; lineIndex++)
                {
                    yield return new WhiteSpaceItem(index, 0, target);
                    yield return null;
                }
                yield return new WhiteSpaceItem(index, lines, target);
            }
            yield return new TokenItem(target);
        }


        sealed class TokenItem : DumpableObject, IItem
        {
            readonly SourceSyntax Target;

            internal TokenItem(SourceSyntax target) { Target = target; }

            internal string Id => Target.Token.Characters.Id;
            protected override string GetNodeDump() => Id.Quote();
        }

        sealed class WhiteSpaceItem : DumpableObject, IItem
        {
            internal readonly int Index;
            internal readonly int LineIndex;
            readonly SourceSyntax Target;

            internal WhiteSpaceItem(int index, int lineIndex, SourceSyntax target)
            {
                Index = index;
                LineIndex = lineIndex;
                Target = target;
            }

            internal string Id
                => Target.Token.PrecededWith[Index].Characters.Id.Split('\n')[LineIndex];
            protected override string GetNodeDump() => Index+"/"+LineIndex+"/"+Id.Quote();
        }
    }
}