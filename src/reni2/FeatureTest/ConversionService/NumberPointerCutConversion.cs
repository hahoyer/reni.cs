using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.UnitTest;
using Reni.Basics;
using Reni.Code;
using Reni.Code.ReplaceVisitor;
using Reni.Context;
using Reni.Struct;
using Reni.Type;
using Reni.Type.ConversionService;

namespace Reni.FeatureTest.ConversionService
{
    [TestFixture]
    public sealed class NumberPointerCutConversion : DependantAttribute
    {
        [Test]
        public void Run()
        {
            var source = new Root(null).BitType.Number(8).EnableCut.Pointer;
            var destination = new Root(null).BitType.Number(6);
            var path = FindPath(source, destination);

            Tracer.Assert(path != null);

            var calulatedDestination = path.Execute(Category.Type).Type;
            Tracer.Assert(calulatedDestination == destination);

            var code = path.Execute(Category.Code).Code.ReplaceArg(source.ArgResult(Category.Code.Typed));

        }
    }
}