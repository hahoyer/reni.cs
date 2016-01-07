using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using Reni.Basics;
using Reni.Type;

namespace Reni.Feature
{
    sealed class Conversion : DumpableObject, IConversion
    {
        static int _nextObjectId;

        internal Conversion(Func<Category, Result> function, TypeBase source)
            : base(_nextObjectId++)
        {
            Function = function;
            Source = source;
            Tracer.Assert(Source != null);
            StopByObjectId(5);
        }

        Func<Category, Result> Function { get; }

        [EnableDump]
        TypeBase Source { get; }

        Result IConversion.Execute(Category category) => Function(category);
        TypeBase IConversion.Source => Source;

        protected override string GetNodeDump()
            => Source.DumpPrintText
                + "-->"
                + (Function(Category.Type).Type?.DumpPrintText ?? "<unknown>")
                + " MethodName="
                + Function.Method.Name;
    }
}