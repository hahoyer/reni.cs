using System;
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
    internal sealed class Type : TypeBase
    {
        private static int _nextObjectId;
        private readonly PositionContainerContext _context;

        [IsDumpEnabled(false)]
        internal readonly ISearchPath<IFeature, Reference> DumpPrintReferenceFeature;

        internal Type(PositionContainerContext context)
            : base(_nextObjectId++)
        {
            _context = context;
        }

        [IsDumpEnabled(false)]
        internal RefAlignParam RefAlignParam { get { return _context.RefAlignParam; } }

        [IsDumpEnabled(false)]
        internal ContextPosition[] Features { get { return _context.Features; } }

        protected override Size GetSize() { return _context.StructSize; }

        internal override string DumpShort() { return "type(" + _context.DumpShort() + ")"; }

        internal override void Search(ISearchVisitor searchVisitor)
        {
            var searchVisitorChild = searchVisitor as SearchVisitor<IFeature>;
            if(searchVisitorChild != null && !searchVisitorChild.IsSuccessFull)
            {
                searchVisitorChild.InternalResult =
                    _context
                        .SearchFromRefToStruct(searchVisitorChild.Defineable)
                        .CheckedConvert(this);
            }
            searchVisitor.ChildSearch(this);
            base.Search(searchVisitor);
        }

        protected override Result ConvertToImplementation(Category category, TypeBase dest)
        {
            Tracer.Assert(dest.IsVoid);
            Tracer.Assert(Size.IsZero);
            return dest.Result
                (
                    category,
                    () => CodeBase.Arg(Size.Zero),
                    () => Context.ConstructorRefs()
                );
        }

        internal override PositionContainerContext GetStruct() { return _context; }

        internal override bool IsConvertableToImplementation(TypeBase dest, ConversionParameter conversionParameter)
        {
            if(dest.IsVoid)
                return Size.IsZero;
            NotImplementedMethod(dest, conversionParameter);
            return false;
        }

        internal Result CreateDumpPrintResult(Category category)
        {
            var argCodes = Context.CreateArgCodes(category);
            var dumpPrint =
                Context.Types
                    .Select((type, i) => type.GenericDumpPrint(category).ReplaceArg(argCodes[i]))
                    .ToArray();
            var thisRef = LocalReferenceResult(category, Context.RefAlignParam);
            var result = Reni.Result
                .ConcatPrintResult(category, dumpPrint)
                .ReplaceArg(thisRef);
            return result;
        }

    }
}