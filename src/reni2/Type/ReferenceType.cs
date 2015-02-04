using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Reni.Basics;
using Reni.Context;
using Reni.Feature;
using Reni.TokenClasses;

namespace Reni.Type
{
    sealed class ReferenceType
        : TypeBase
            , ISymbolProviderForPointer<Mutable, IFeatureImplementation>
            , ISymbolProviderForPointer<EnableReinterpretation, IFeatureImplementation>
    {
        readonly int _order;

        internal sealed class Options : DumpableObject
        {
            OptionsData OptionsData { get; }

            Options(string optionsId)
            {
                OptionsData = new OptionsData(optionsId);
                IsMutable = new OptionsData.Option(OptionsData);
                IsOverSizeable = new OptionsData.Option(OptionsData);
                IsEnableReinterpretation = new OptionsData.Option(OptionsData);
                OptionsData.Align();
                Tracer.Assert(OptionsData.IsValid);
            }

            internal OptionsData.Option IsMutable { get; }
            internal OptionsData.Option IsOverSizeable { get; }
            internal OptionsData.Option IsEnableReinterpretation { get; }

            internal static Options Create(string optionsId = null) => new Options(optionsId);
            internal static readonly string DefaultOptionsId = Create().OptionsData.Id;
            protected override string GetNodeDump()
                => (IsMutable.Value ? "m" : "");
        }

        internal ReferenceType(ArrayType valueType, string optionsId)
        {
            options = Options.Create(optionsId);
            _order = CodeArgs.NextOrder++;
            ValueType = valueType;
            Tracer.Assert(!valueType.Hllw, valueType.Dump);
            Tracer.Assert(!(valueType.CoreType is PointerType), valueType.Dump);
            StopByObjectId(-10);
        }

        internal ArrayType ValueType { get; }
        Options options { get; }

        [DisableDump]
        internal override Root RootContext => ValueType.RootContext;
        internal override string DumpPrintText => "(" + ValueType.DumpPrintText + ")reference";
        [DisableDump]
        internal override bool Hllw => false;
        [DisableDump]
        internal override bool IsAligningPossible => false;

        protected override string GetNodeDump() => ValueType.NodeDump + "[reference]";
        protected override Size GetSize() => Root.DefaultRefAlignParam.RefSize;

        [DisableDump]
        internal ReferenceType Mutable => ValueType.Reference(options.IsMutable.SetTo(true));
        [DisableDump]
        internal ReferenceType OverSizeable => ValueType.Reference(options.IsOverSizeable.SetTo(true));
        [DisableDump]
        internal ReferenceType EnableReinterpretation => ValueType.Reference(options.IsEnableReinterpretation.SetTo(true));


        IFeatureImplementation ISymbolProviderForPointer<Mutable, IFeatureImplementation>.Feature(Mutable tokenClass)
            => Extension.SimpleFeature(MutableResult);

        IFeatureImplementation ISymbolProviderForPointer<EnableReinterpretation, IFeatureImplementation>.Feature
            (EnableReinterpretation tokenClass)
            => Extension.SimpleFeature(EnableReinterpretationResult);


        Result MutableResult(Category category)
        {
            Tracer.Assert(ValueType.IsMutable);
            return ResultFromPointer(category, Mutable);
        }

        Result EnableReinterpretationResult(Category category) => ResultFromPointer(category, EnableReinterpretation);
    }
}