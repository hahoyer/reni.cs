using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Reni.Basics;
using Reni.Code;
using Reni.Type;

namespace Reni.Feature
{
    sealed class Value : DumpableObject, IImplementation, IValue
    {
        static int _nextObjectId;

        internal Value(Func<Category, Result> function, TypeBase source)
            : base(_nextObjectId++)
        {
            Function = function;
            Source = source;
            Tracer.Assert(Source != null);
        }

        [EnableDump]
        internal Func<Category, Result> Function { get; }
        TypeBase Source { get; }

        Result IValue.Execute(Category category) => Function(category);

        ResultCache.IResultProvider IValue.FindSource(IContextReference ext)
        {
            NotImplementedMethod(ext);
            return null;
        }

        protected override string GetNodeDump()
            => Source.DumpPrintText
                + "-->"
                + (Function(Category.Type).Type?.DumpPrintText ?? "<unknown>")
                + " MethodName="
                + Function.Method.Name;


        IMeta IMetaImplementation.Function => null;
        IFunction IEvalImplementation.Function => null;
        IValue IEvalImplementation.Value => this;
    }
}