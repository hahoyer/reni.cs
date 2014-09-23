using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Helper;
using Reni.Basics;
using Reni.Context;
using Reni.Feature;
using Reni.Feature.DumpPrint;
using Reni.Sequence;

namespace Reni.Type
{
    sealed class NumberType
        : TypeBase
            , ISymbolProvider<DumpPrintToken, IFeatureImplementation>
            , ISymbolProvider<TokenClasses.EnableCut, IFeatureImplementation>
            , IConverterProvider<NumberType, IFeatureImplementation>
    {
        readonly FunctionCache<RefAlignParam, ObjectReference> _objectReferencesCache;
        readonly ArrayType _parent;

        public NumberType(ArrayType parent)
        {
            _parent = parent;
            _objectReferencesCache = new FunctionCache<RefAlignParam, ObjectReference>
                (refAlignParam => new ObjectReference(this, refAlignParam));
        }

        [DisableDump]
        internal override Root RootContext { get { return _parent.RootContext; } }
        protected override Size GetSize() { return _parent.Size; }
        [DisableDump]
        internal override bool Hllw { get { return _parent.Hllw; } }
        [EnableDump]
        internal int Bits { get { return Size.ToInt(); } }
        [DisableDump]
        protected override IEnumerable<IGenericProviderForType> Genericize { get { return this.GenericList(base.Genericize); } }

        internal ObjectReference UniqueObjectReference(RefAlignParam refAlignParam)
        {
            return _objectReferencesCache[refAlignParam];
        }

        IFeatureImplementation ISymbolProvider<DumpPrintToken, IFeatureImplementation>.Feature
        {
            get { return Extension.Feature(DumpPrintTokenResult); }
        }


        IFeatureImplementation ISymbolProvider<TokenClasses.EnableCut, IFeatureImplementation>.Feature
        {
            get { return Extension.Feature(EnableCutTokenResult); }
        }

        IFeatureImplementation IConverterProvider<NumberType, IFeatureImplementation>.Feature
            (NumberType destination, IConversionParameter parameter)
        {
            if(!parameter.EnableCut && Bits > destination.Bits)
                return null;
            return Extension.Feature(ca => ConversionAsReference(ca, destination));
        }

        Result DumpPrintTokenResult(Category category) { return VoidType.Result(category, DumpPrintNumberCode, CodeArgs.Arg); }
        Result EnableCutTokenResult(Category category) { return UniqueEnableCutType.UniquePointer.ArgResult(category.Typed); }

        Result ConversionAsReference(Category category, NumberType destination)
        {
            return destination
                .FlatConversion(category, this)
                .ReplaceArg(UnalignedDereferencePointerResult)
                .LocalPointerKindResult;
        }

        Result FlatConversion(Category category, NumberType source)
        {
            if(Bits == source.Bits)
                return ArgResult(category.Typed);

            return Result
                (
                    category,
                    () => source.ArgCode.BitCast(Size),
                    CodeArgs.Arg
                );
        }

        Result UnalignedDereferencePointerResult(Category category)
        {
            return PointerKind.ArgResult(category.Typed).DereferenceResult & category;
        }
    }
}