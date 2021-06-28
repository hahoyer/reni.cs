using System;
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
            StopByObjectIds();
        }

        Func<Category, Result> Function { get; }

        [EnableDump]
        TypeBase Source { get; }
        [EnableDump]
        TypeBase Destination => Function(Category.Type).Type;

        Result IConversion.Execute(Category category) => Function(category);
        TypeBase IConversion.Source => Source;

        protected override string GetNodeDump()
            => Source.DumpPrintText
                + "-->"
                + (Destination?.DumpPrintText ?? "<unknown>")
                + " MethodName="
                + Function.Method.Name;
    }
}