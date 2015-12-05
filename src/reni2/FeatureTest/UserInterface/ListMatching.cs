using System;
using System.Collections.Generic;
using hw.Helper;
using System.Linq;
using hw.Debug;
using hw.UnitTest;

namespace Reni.FeatureTest.UserInterface
{
    [UnitTest]
    public sealed class ListMatching : DependantAttribute
    {
        [UnitTest]
        public void Matching()
        {
            const string Text = @"(1,3,4,6)";
            var compiler = Compiler.BrowserFromText(text: Text);
            var comma = compiler.LocatePosition(2);
            var commas = comma.FindAllBelongings(compiler).ToArray();
            Tracer.Assert(commas.Length == 3);
        }

        [UnitTest]
        public void CombinationsOfMatching()
        {
            const string Text = @"(1,3,4,6)";
            var compiler = Compiler.BrowserFromText(text: Text);

            var commas = Text
                .Select
                (
                    (item, index) => new
                    {
                        item,
                        index
                    })
                .Where(item => item.item == ',')
                .Select(item => compiler.LocatePosition(item.index).FindAllBelongings(compiler).ToArray())
                .ToArray();

            Tracer.Assert(commas.Length == 3);
            var combis = commas
                .Skip(1)
                .Select(comma => comma.Merge(commas[0], item => item).ToArray())
                .ToArray();

            foreach(var combi in combis)
                Tracer.Assert(combi.Length == 3);
        }

        [UnitTest]
        public void MixedMatching()
        {
            const string Text = @"(1,3,4;2,6)";
            var compiler = Compiler.BrowserFromText(text: Text);

            var commas = Text
                .Select
                (
                    (item, index) => new
                    {
                        item,
                        index
                    })
                .Where(item => item.item == ',')
                .Select(item => compiler.LocatePosition(item.index).FindAllBelongings(compiler).ToArray())
                .ToArray();

            Tracer.Assert(commas.Length == 3);
            Tracer.Assert(commas[0].Length == 2);
            Tracer.Assert(commas[1].Length == 2);
            Tracer.Assert(commas[2].Length == 1);

            var firstPart = commas[0].Merge(commas[1], item => item).ToArray();

            Tracer.Assert(firstPart.Length == 2);
        }
    }
}