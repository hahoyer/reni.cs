using System;
using hw.DebugFormatter;
using Reni.Basics;
using Reni.Type;

namespace Reni.Feature
{
    sealed class Value
        : DumpableObject
            , IImplementation
            , IValue
    {
        static int _nextObjectId;

        [EnableDump]
        internal Func<Category, Result> Function { get; }

        TypeBase Source { get; }

        internal Value(Func<Category, Result> function, TypeBase source)
            : base(_nextObjectId++)
        {
            Function = function;
            Source = source;
            Tracer.Assert(Source != null);
        }

        IFunction IEvalImplementation.Function => null;
        IValue IEvalImplementation.Value => this;

        IMeta IMetaImplementation.Function => null;

        Result IValue.Execute(Category category) => Function(category);

        protected override string GetNodeDump()
            => Source.DumpPrintText
               + "-->"
               + (Function(Category.Type).Type?.DumpPrintText ?? "<unknown>")
               + " MethodName="
               + Function.Method.Name;
    }
}