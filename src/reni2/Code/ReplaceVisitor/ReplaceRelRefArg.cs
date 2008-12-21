using System;
using HWClassLibrary.Debug;
using Reni.Context;

namespace Reni.Code.ReplaceVisitor
{
    internal sealed class ReplaceRelRefArg : ReplaceArg
    {
        private readonly RefAlignParam _refAlignParam;

        [DumpData(true)]
        private readonly Size _offset;

        private ReplaceRelRefArg(CodeBase actual, RefAlignParam refAlignParam, Size offset)
            : base(actual)
        {
            _refAlignParam = refAlignParam;
            _offset = offset;
            //StopByObjectId(2188);
        }

        internal ReplaceRelRefArg(CodeBase actualArg, RefAlignParam refAlignParam)
            : this(actualArg, refAlignParam, Size.Create(0)) {}

        public RefAlignParam RefAlignParam { get { return _refAlignParam; } }

        public override CodeBase Actual
        {
            get
            {
                if(_offset.IsZero)
                    return ActualArg;
                return ActualArg.CreateRefPlus(RefAlignParam, Offset);
            }
        }

        public Size Offset { get { return _offset; } }

        internal override Visitor<CodeBase> After(Size size)
        {
            return new ReplaceRelRefArg(ActualArg, RefAlignParam, Offset + size);
        }
    }
}