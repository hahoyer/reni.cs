using System;
using HWClassLibrary.Debug;
using NUnit.Framework;
using Reni.Parser;
using Reni.Struct;
using Reni.Syntax;

namespace Reni.FeatureTest
{
    /// <summary>
    /// Operations on bitarrays
    /// </summary>
    [TestFixture]
    public class ComilerBasics : CompilerTest
    {
        [Test, Category(Worked)]
        public void Add2Numbers()
        {
            var syntaxPrototype = Structure.Syntax.Expression(Structure.Syntax.Expression(Structure.Syntax.Number(2), "+", Structure.Syntax.Number(4)), "dump_print");
            Parameters.ParseOnly = true;
            RunCompiler("Add2Numbers", @"(2+4) dump_print", c => syntaxPrototype.AssertLike(c.Syntax));
        }

        [Test, Category(UnderConstruction)]
        public void UseAlternativePrioTable()
        {
            var syntaxPrototype = Structure.Syntax.Struct();
            Parameters.ParseOnly = true;
            RunCompiler("UseAlternativePrioTable", @"!property x: 3", c => syntaxPrototype.AssertLike(c.Syntax));
        }
    }
}