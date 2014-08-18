using System;
using System.Collections.Generic;
using System.Linq;
using hw.UnitTest;

namespace Reni.FeatureTest.Array
{
    [TestFixture]
    [SimpleArrayFromPiece]
    [Target("(<<5<<3<<5<<1) dump_print")]
    [Output("array(#(#align3#)# (bit)sequence(4),(5, 3, 5, 1))")]
    public sealed class ArrayFromPieces : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }
}