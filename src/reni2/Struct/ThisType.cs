using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Type;

namespace Reni.Struct
{
    internal abstract class ThisType<TContext> : TypeBase
        where TContext : Context
    {
        private readonly Type<TContext> _type;
        
        protected ThisType(Type<TContext> type)
        {
            _type = type;
        }

        protected override Size GetSize() { return _type.RefAlignParam.RefSize; }
        internal override string DumpShort() { return "type(this)"; }
    }

    internal sealed class ThisTypeX : TypeBase
    {
        [DumpData(true)]
        private readonly Context _context;


        [DumpData(false)]
        internal readonly IFeature DumpPrintFeature;

        internal ThisTypeX(Context context)
        {
            _context = context; 
            DumpPrintFeature = new Feature.DumpPrint.StructFeature(this);
        }

        protected override Result ConvertTo_Implementation(Category category, TypeBase dest)
        {
            var fullContext = Context as FullContext;
            if (fullContext == null)
                return base.ConvertTo_Implementation(category, dest);
            Tracer.Assert(dest.IsVoid);
            Tracer.Assert(Size.IsZero);
            return dest.CreateResult
                (
                category,
                () => CodeBase.CreateArg(Size.Zero),
                fullContext.ConstructorRefs
                );
        }

        internal override bool IsConvertableTo_Implementation(TypeBase dest, ConversionFeature conversionFeature)
        {
            var fullContext = Context as FullContext;
            if (fullContext == null)
                return base.IsConvertableTo_Implementation(dest,conversionFeature);
            if (dest.IsVoid)
                return Size.IsZero;
            NotImplementedMethod(dest, conversionFeature);
            return false;
        }

        internal override void Search(ISearchVisitor searchVisitor)
        {
            var searchVisitorChild = searchVisitor as SearchVisitor<ISearchPath<IFeature, Ref>>;
            if (searchVisitorChild != null)
                searchVisitorChild.InternalResult =
                    Context.Container.SearchFromRefToStruct(searchVisitorChild.Defineable).CheckedConvert(this);
            searchVisitor.Child(this).Search();
            base.Search(searchVisitor);
        }

        protected override Size GetSize() { return _context.RefSize; }
        protected override ThisType GetThisType() { return this; }
        internal override string DumpShort() { return "type(this)"; }

        internal StructRef At(int position) { return new StructRef(_context, position); }
        internal Result AtResult(Category category, int position) { return At(position).CreateArgResult(category); }
        internal Context Context { get { return _context; } }

        internal Result DumpPrint(Category category)
        {
            var argCodes = CreateArgCodes(category);
            var dumpPrint =
                _context.Types
                    .Select((type, i) => type.GenericDumpPrint(category).UseWithArg(argCodes[i]))
                    .ToArray();
            var thisRef = CreateArgResult(category)
                .CreateAutomaticRefResult(_context.RefAlignParam);
            var result = Result
                .ConcatPrintResult(category, dumpPrint)
                .UseWithArg(thisRef);
            return result;
        }

        [DumpData(false)]
        internal TypeBase IndexType { get { return _context.IndexType; } }
        
        private Result[] CreateArgCodes(Category category)
        {
            return _context.Types
                .Select((type, i) => AutomaticDereference(type, _context.Offsets[i], category))
                .ToArray();
        }

        private Result AutomaticDereference(TypeBase type, Size offset, Category category)
        {
            return type
                .CreateAutomaticRef(_context.RefAlignParam)
                .CreateResult(category, () => CreateRefArgCode().CreateRefPlus(_context.RefAlignParam, offset))
                .AutomaticDereference();
        }

        private CodeBase CreateRefArgCode() { return CreateAutomaticRef(_context.RefAlignParam).CreateArgCode(); }

    }

}