using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Feature.DumpPrint;
using Reni.Type;

namespace Reni.Struct
{
    internal sealed class Type : TypeBase
    {
        private static int _nextObjectId;
        private readonly PositionContainerContext _positionContainerContext;

        [DisableDump]
        internal readonly ISearchPath<IFeature, Reference> DumpPrintReferenceFeature;

        internal Type(PositionContainerContext positionContainerContext)
            : base(_nextObjectId++)
        {
            _positionContainerContext = positionContainerContext;
            DumpPrintReferenceFeature = new StructReferenceFeature(this);
        }

        [DisableDump]
        internal RefAlignParam RefAlignParam { get { return PositionContainerContext.RefAlignParam; } }

        [DisableDump]
        internal ContainerContext ContainerContext { get { return PositionContainerContext.ContainerContext; } }

        [DisableDump]
        private PositionContainerContext PositionContainerContext { get { return _positionContainerContext; } }

        protected override Size GetSize() { return PositionContainerContext.StructSize; }

        internal override string DumpShort() { return "type(" + PositionContainerContext.DumpShort() + ")"; }

        internal override void Search(ISearchVisitor searchVisitor)
        {
            var searchVisitorChild = searchVisitor as SearchVisitor<IFeature>;
            if(searchVisitorChild != null && !searchVisitorChild.IsSuccessFull)
            {
                searchVisitorChild.InternalResult =
                    PositionContainerContext
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
                    () => PositionContainerContext.ConstructorRefs
                );
        }

        internal override PositionContainerContext GetStruct() { return PositionContainerContext; }

        internal override bool IsConvertableToImplementation(TypeBase dest, ConversionParameter conversionParameter)
        {
            if(dest.IsVoid)
                return Size.IsZero;
            NotImplementedMethod(dest, conversionParameter);
            return false;
        }

        internal Result DumpPrintResult(Category category)
        {
            var argCodes = PositionContainerContext.CreateArgCodes(category);
            var containerContext = PositionContainerContext.ContainerContext;
            var dumpPrint =
                argCodes
                    .Select((code, i) => containerContext
                        .InnerType(i)
                        .GenericDumpPrint(category)
                        .ReplaceArg(code))
                    .ToArray();
            var thisRef = LocalReferenceResult(category, RefAlignParam);
            var result = Reni.Result
                .ConcatPrintResult(category, dumpPrint)
                .ReplaceArg(thisRef);
            return result;
        }

    }
}