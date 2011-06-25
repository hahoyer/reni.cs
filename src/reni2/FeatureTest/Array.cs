using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.UnitTest;
using Reni.FeatureTest.Integer;

namespace Reni.FeatureTest.Array
{
    [TestFixture]
    [IntegerPlusInteger]
    [Target("(()<*5<*3<*5<*1) dump_print")]
    [Output("array(4 bits,(5,3,5,1))")]
    public sealed class ArrayFromPieces : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [IntegerPlusInteger]
    [Target("(()<*5<*3)<<(()<*5<*1) dump_print")]
    [Output("array(4 bits,(5,3,5,1))")]
    public sealed class CombineArraysFromPieces : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [IntegerPlusInteger]
    [Target("(5 type * 5)(arg/\\) array dump_print")]
    [Output("array(4 bits,(0,1,2,3,4))")]
    public sealed class FromTypeAndFunction : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }
}