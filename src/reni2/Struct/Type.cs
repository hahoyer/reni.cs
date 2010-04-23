using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Type;

namespace Reni.Struct
{
    [Serializable]
    internal abstract class Type<TContext> : TypeBase
        where TContext : Context
    {
        private bool _isGetSizeActive;

        [Node]
        internal readonly TContext Context;

        internal Type(TContext context)
        {
            Context = context;
        }

        protected override Size GetSize()
        {
            if(_isGetSizeActive)
                return Size.Create(-1);
            _isGetSizeActive = true;
            var result = Context.InternalSize();
            _isGetSizeActive = false;
            return result;
        }

        internal override string DumpShort() { return "type." + ObjectId + "(context." + Context.DumpShort() + ")"; }

        protected internal override int IndexSize { get { return Context.IndexSize; } }

        internal override bool IsConvertableTo_Implementation(TypeBase dest, ConversionFeature conversionFeature)
        {
            if(dest.IsVoid)
                return Size.IsZero;
            NotImplementedMethod(dest, conversionFeature);
            return false;
        }

        internal RefAlignParam RefAlignParam { get { return Context.RefAlignParam; } }

    }

    internal sealed class FullContextType : Type<FullContext>
   {
        public FullContextType(FullContext context)
            : base(context) { }

        protected override Result ConvertTo_Implementation(Category category, TypeBase dest)
        {
            Tracer.Assert(dest.IsVoid);
            Tracer.Assert(Size.IsZero);
            return dest.CreateResult
                (
                    category,
                    () => CodeBase.CreateArg(Size.Zero),
                    () => Context.ConstructorRefs()
                );
        }
    }
    internal sealed class ContextAtPositionType : Type<ContextAtPosition>
    {
        public ContextAtPositionType(ContextAtPosition context)
            : base(context) { }

    }
}