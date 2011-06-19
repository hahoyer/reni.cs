﻿using System;
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
        internal AccessPoint AccessPoint { get { return _accessPoint; } }

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

        internal CodeBase LocalCode()
        {
            return LocalReferenceCode(RefAlignParam)
                .Dereference(RefAlignParam, Size);
        }

        internal Result DumpPrintResult(Category category)
        {
            return AccessPoint
                .DumpPrintResult(category, LocalReferenceResult(category, RefAlignParam));
        }
    }
}