using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Feature.DumpPrint;
using Reni.Type;

namespace Reni.Struct
{
    internal class Type : TypeBase
    {
        private static int _nextObjectId;

        [DumpData(false)]
        private readonly Context _context;

        [DumpData(false)]
        internal readonly ISearchPath<IFeature, Reni.Type.Reference> DumpPrintReferenceFeature;

        internal Type(Context context)
            : base(_nextObjectId++)
        {
            _context = context;
            DumpPrintReferenceFeature = new StructReferenceFeature(this);
        }

        internal RefAlignParam RefAlignParam { get { return _context.RefAlignParam; } }
        internal Context Context { get { return _context; } }

        protected override Size GetSize() { return _context.InternalSize(); }
        internal override string DumpShort() { return "type(" + _context.DumpShort() + ")"; }

        internal override void Search(ISearchVisitor searchVisitor)
        {
            var searchVisitorChild = searchVisitor as SearchVisitor<IFeature>;
            if(searchVisitorChild != null && !searchVisitorChild.IsSuccessFull)
            {
                searchVisitorChild.InternalResult =
                    _context
                        .Container
                        .SearchFromRefToStruct(searchVisitorChild.Defineable)
                        .CheckedConvert(this);
            }
            searchVisitor.ChildSearch(this);
            base.Search(searchVisitor);
        }

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
        internal override Context GetStruct() { return _context; }

        internal override bool IsConvertableTo_Implementation(TypeBase dest, ConversionFeature conversionFeature)
        {
            if (dest.IsVoid)
                return Size.IsZero;
            NotImplementedMethod(dest, conversionFeature);
            return false;
        }

        internal Result CreateDumpPrintResult(Category category)
        {
            var argCodes = Context.CreateArgCodes(category);
            var dumpPrint =
                Context.Types
                    .Select((type, i) => type.GenericDumpPrint(category).ReplaceArg(argCodes[i]))
                    .ToArray();
            var thisRef = CreateArgResult(category)
                .CreateAutomaticRefResult(Context.RefAlignParam);
            var result = Result
                .ConcatPrintResult(category, dumpPrint)
                .ReplaceArg(thisRef);
            return result;
        }
    }
}