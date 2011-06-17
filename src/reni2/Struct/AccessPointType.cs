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
    internal sealed class AccessPointType : TypeBase
    {
        private static int _nextObjectId;
        private readonly AccessPoint _accessPoint;

        [DisableDump]
        internal readonly ISearchPath<IFeature, Reference> DumpPrintReferenceFeature;

        internal AccessPointType(AccessPoint accessPoint)
            : base(_nextObjectId++)
        {
            _accessPoint = accessPoint;
            DumpPrintReferenceFeature = new StructReferenceFeature(this);
        }

        [DisableDump]
        internal RefAlignParam RefAlignParam { get { return AccessPoint.RefAlignParam; } }

        [DisableDump]
        internal ContainerContextObject ContainerContextObject { get { return AccessPoint.ContainerContextObject; } }

        [DisableDump]
        private AccessPoint AccessPoint { get { return _accessPoint; } }

        protected override Size GetSize() { return AccessPoint.StructSize; }

        internal override string DumpShort() { return "type(" + AccessPoint.DumpShort() + ")"; }

        internal override void Search(ISearchVisitor searchVisitor)
        {
            var searchVisitorChild = searchVisitor as SearchVisitor<IFeature>;
            if(searchVisitorChild != null && !searchVisitorChild.IsSuccessFull)
            {
                searchVisitorChild.InternalResult =
                    AccessPoint
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
                    () => AccessPoint.ConstructorRefs
                );
        }

        internal override AccessPoint GetStructAccessPoint() { return AccessPoint; }

        internal override bool IsConvertableToImplementation(TypeBase dest, ConversionParameter conversionParameter)
        {
            if(dest.IsVoid)
                return Size.IsZero;
            NotImplementedMethod(dest, conversionParameter);
            return false;
        }

        internal Result DumpPrintResult(Category category)
        {
            var argCodes = AccessPoint.CreateArgCodes(category);
            var containerContext = AccessPoint.ContainerContextObject;
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

        internal CodeBase LocalCode()
        {
            return LocalReferenceCode(RefAlignParam)
                .Dereference(RefAlignParam, Size);
        }

    }
}