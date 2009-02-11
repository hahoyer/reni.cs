using System;
using HWClassLibrary.Debug;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Parser.TokenClass;

namespace Reni.Type
{
    /// <summary>
    /// Performs alignement by extending the number of bytes a type uses.
    /// </summary>
    [Serializable]
    internal class Aligner : Child
    {
        private readonly int _alignBits;

        public Aligner(TypeBase target, int alignBits)
            : base(target)
        {
            _alignBits = alignBits;
            StopByObjectId(1117);
        }

        internal int AlignBits { get { return _alignBits; } }
        internal override string DumpPrintText { get { return "#(#align" + _alignBits + "#)# " + Parent.DumpPrintText; } }

        [DumpData(false)]
        internal override int SequenceCount { get { return Parent.SequenceCount; } }
        protected override Size GetSize() { return Parent.Size.Align(AlignBits); }
        internal override Size UnrefSize { get { return Parent.UnrefSize; } }
        internal override AutomaticRef CreateAutomaticRef(RefAlignParam refAlignParam) { return Parent.CreateAutomaticRef(refAlignParam.Align(AlignBits)); }
        internal override AssignableRef CreateAssignableRef(RefAlignParam refAlignParam) { return Parent.CreateAssignableRef(refAlignParam.Align(AlignBits)); }
        internal override Result Destructor(Category category) { return Parent.Destructor(category); }
        internal override Result Copier(Category category) { return Parent.Copier(category); }
        internal override Result DumpPrintFromRef(Category category, RefAlignParam refAlignParam) { return Parent.DumpPrintFromRef(category, refAlignParam); }
        internal override Result ApplyTypeOperator(Result argResult) { return Parent.ApplyTypeOperator(argResult); }
        internal override bool IsConvertableToImplementation(TypeBase dest, ConversionFeature conversionFeature) { return Parent.IsConvertableTo(dest, conversionFeature); }

        internal override SearchResult<IFeature> Search(Defineable defineable) { return Parent.Search(defineable); }
        internal override SearchResult<IPrefixFeature> SearchPrefix(Defineable defineable) { return Parent.SearchPrefix(defineable); }
        internal override SearchResult<IConverter<IFeature, Ref>> SearchFromRef(Defineable defineable) { return Parent.SearchFromRef(defineable); }
        internal override SearchResult<IConverter<IFeature, Sequence>> SearchFromSequence(Defineable defineable) { return Parent.SearchFromSequence(defineable); }
        internal override SearchResult<IConverter<IPrefixFeature, Sequence>> SearchPrefixFromSequence(Defineable defineable) { return Parent.SearchPrefixFromSequence(defineable); }
        internal override SearchResult<IConverter<IConverter<IFeature, Ref>, Sequence>> SearchFromRefToSequence(Defineable defineable) { return Parent.SearchFromRefToSequence(defineable); }

        internal override Result ConvertToImplementation(Category category, TypeBase dest)
        {
            return Parent
                .ConvertTo(category, dest)
                .UseWithArg(CreateUnalignedArgResult(category));
        }

        private Result CreateUnalignedArgResult(Category category)
        {
            return Parent.CreateResult
                (
                category,
                () => CodeBase.CreateArg(Size).CreateBitCast(Parent.Size)
                );
        }

        internal override bool HasConverterTo(TypeBase dest) { return Parent.HasConverterTo(dest); }

        internal override string DumpShort() { return "aligner(" + Parent.DumpShort() + ")"; }
    }
}