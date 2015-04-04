using System;
using System.Collections.Generic;
using hw.Helper;
using System.Linq;
using hw.Debug;
using hw.UnitTest;

namespace Reni.FeatureTest.UserInterface
{
    [UnitTest]
    public sealed class ThenElseMatching : DependantAttribute
    {
        [UnitTest]
        public void Matching()
        {
            const string Text = @"1 then 2 else 3";
            var compiler = new Compiler(text: Text);
            var thenToken = compiler.Token(2);
            var elseToken = compiler.Token(Text.IndexOf("else"));

            var thenMatches = thenToken.FindAllBelongings(compiler).ToArray();
            var elseMatches = elseToken.FindAllBelongings(compiler).ToArray();

            Tracer.Assert(elseMatches.Length == 2);
            Tracer.Assert(thenMatches.Length == 2);
            Tracer.Assert(thenMatches.Merge(elseMatches, i => i).ToArray().Length == 2);
        }

        [UnitTest]
        public void NestedMatching()
        {
            const string Text = @"1 then 2 then 333 else 3";
            var compiler = new Compiler(text: Text);
            var thenToken = compiler.Token(2);
            var elseToken = compiler.Token(Text.IndexOf("else"));

            var thenMatches = thenToken.FindAllBelongings(compiler).ToArray();
            var elseMatches = elseToken.FindAllBelongings(compiler).ToArray();

            Tracer.Assert(elseMatches.Length == 2);
            Tracer.Assert(thenMatches.Length == 2);
            Tracer.Assert(thenMatches.Merge(elseMatches, i => i).ToArray().Length == 2);
        }
    }
}