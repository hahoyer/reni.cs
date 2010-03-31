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

        [DumpData(false)]
        internal readonly IFeature DumpPrintFeature;

        internal Type(TContext context)
        {
            Context = context;
            DumpPrintFeature = new Feature.DumpPrint.StructFeature<TContext>(this);
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

        internal override void Search(ISearchVisitor searchVisitor)
        {
            var searchVisitorChild = searchVisitor as SearchVisitor<ISearchPath<IFeature, Ref>>;
            if(searchVisitorChild != null)
                searchVisitorChild.InternalResult =
                    Context.Container.SearchFromRefToStruct(searchVisitorChild.Defineable).CheckedConvert(this);
            searchVisitor.Child(this).Search();
            base.Search(searchVisitor);
        }

        internal RefAlignParam RefAlignParam { get { return Context.RefAlignParam; } }

        internal Result DumpPrint(Category category)
        {
            var argCodes = CreateArgCodes(category);
            var dumpPrint =
                Context.Types
                    .Select((type, i) => type.GenericDumpPrint(category).UseWithArg(argCodes[i]))
                    .ToArray();
            var thisRef = CreateArgResult(category)
                .CreateAutomaticRefResult(Context.RefAlignParam);
            var result = Result
                .ConcatPrintResult(category, dumpPrint)
                .UseWithArg(thisRef);
            return result;
        }

        private Result[] CreateArgCodes(Category category)
        {
            return Context.Types
                .Select((type, i) => AutomaticDereference(type, Context.Offsets[i], category))
                .ToArray();
        }

        private Result AutomaticDereference(TypeBase type, Size offset, Category category)
        {
            return type
                .CreateAutomaticRef(Context.RefAlignParam)
                .CreateResult(category, () => CreateRefArgCode().CreateRefPlus(Context.RefAlignParam, offset))
                .AutomaticDereference();
        }

        private CodeBase CreateRefArgCode() { return CreateAutomaticRef(Context.RefAlignParam).CreateArgCode(); }
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