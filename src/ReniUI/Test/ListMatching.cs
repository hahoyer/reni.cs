using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.UnitTest;

namespace ReniUI.Test
{
    [UnitTest]
    [PairedSyntaxTree]
    public sealed class ListMatching : DependenceProvider
    {
        [UnitTest]
        public void Matching()
        {
            const string text = @"(1,3,4,6)";
            var compiler = CompilerBrowser.FromText(text);
            var comma = compiler.LocatePosition(2);
            var commas = comma.ParserLevelGroup.ToArray();
            (commas.Length == 2).Assert();
        }

        [UnitTest]
        public void CombinationsOfMatching()
        {
            const string text = @"(1,3,4,6)";
            var compiler = CompilerBrowser.FromText(text);

            var commas = text
                .Select
                (
                    (item, index) => new
                    {
                        item, index
                    })
                .Where(item => item.item == ',')
                .Select
                    (item => compiler.LocatePosition(item.index).ParserLevelGroup.ToArray())
                .ToArray();

            (commas.Length == 3).Assert();
            var tuples = commas
                .Skip(1)
                .Select(comma => comma.Merge(commas[0], item => item).ToArray())
                .ToArray();

            foreach(var tuple in tuples)
                (tuple.Length == 3).Assert();
        }

        [UnitTest]
        public void MixedMatching()
        {
            const string text = @"(1,3,4;2,6)";
            var compiler = CompilerBrowser.FromText(text);

            var commas = text
                .Select((item, index) => new {item, index})
                .Where(item => item.item == ',')
                .Select
                    (item => compiler.LocatePosition(item.index).ParserLevelGroup.ToArray())
                .ToArray();

            (commas.Length == 3).Assert();
            (commas[0].Length == 1).Assert();
            (commas[1].Length == 1).Assert();
            (commas[2].Length == 0).Assert();

            var firstPart = commas[0].Merge(commas[1], item => item).ToArray();

            (firstPart.Length == 2).Assert();
        }
    }
}