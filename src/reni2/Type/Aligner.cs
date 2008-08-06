using System;
using Reni.Code;
using Reni.Context;
using HWClassLibrary.Debug;

namespace Reni.Type
{
    /// <summary>
    /// Performs alignement by extending the number of bytes a type uses.
    /// </summary>
    [Serializable]
    internal class Aligner : Child
    {
        private readonly int _alignBits;

        public Aligner(TypeBase target, int alignBits) : base(target)
        {
            _alignBits = alignBits;
            StopByObjectId(1117);
        }

        internal int AlignBits { get { return _alignBits; } }
        internal override string DumpPrintText { get { return "#(#align" + _alignBits + "#)# " + Parent.DumpPrintText; } }
        
        [DumpData(false)]
        internal override int SequenceCount { get { return Parent.SequenceCount; } }

        protected override Size GetSize()
        {
            return Parent.Size.Align(AlignBits);
        }

        internal override AutomaticRef CreateAutomaticRef(RefAlignParam refAlignParam)
        {
            return Parent.CreateAutomaticRef(refAlignParam.Align(AlignBits));
        }

        internal override AssignableRef CreateAssignableRef(RefAlignParam refAlignParam)
        {
            return Parent.CreateAssignableRef(refAlignParam.Align(AlignBits));
        }

        internal override Result DestructorHandler(Category category)
        {
            return Parent.DestructorHandler(category);
        }

        internal override Result MoveHandler(Category category)
        {
            return Parent.MoveHandler(category);
        }

        internal override Result DumpPrintFromRef(Category category, RefAlignParam refAlignParam)
        {
            return Parent.DumpPrintFromRef(category, refAlignParam);
        }

        internal override Result ApplyTypeOperator(Result argResult)
        {
            return Parent.ApplyTypeOperator(argResult);
        }

        internal override bool IsConvertableToVirt(TypeBase dest, ConversionFeature conversionFeature)
        {
            return Parent.IsConvertableTo(dest, conversionFeature);
        }

        internal override Result ConvertToVirt(Category category, TypeBase dest)
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
                () => CodeBase.CreateArg(GetSize()).CreateBitCast(Parent.Size)
                );
        }

        internal override bool HasConverterTo(TypeBase dest)
        {
            return Parent.HasConverterTo(dest);
        }

        internal override string DumpShort()
        {
            return "aligner(" + Parent.DumpShort() + ")";
        }
    }
}