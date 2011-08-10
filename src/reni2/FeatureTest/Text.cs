using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using HWClassLibrary.UnitTest;
using Reni.FeatureTest.Array;
using Reni.FeatureTest.Function;
using Reni.FeatureTest.Integer;

namespace Reni.FeatureTest.Text
{
    [TestFixture]
    [TargetSet(@"'Hallo' dump_print","Hallo")]
    [IntegerPlusInteger]
    [FunctionVariable]
    [ArrayFromPieces]
    [CombineArraysFromPieces]
    public sealed class Text : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
        
    }
}