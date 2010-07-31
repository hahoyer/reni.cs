using System;
using HWClassLibrary.Debug;
using Reni.Context;

namespace Reni.Code.ReplaceVisitor
{
    internal sealed class ReplaceRelRefArg : ReplaceArg
    {
        [DumpData(true)]
        private readonly Size _offset;

        private ReplaceRelRefArg(CodeBase actual, Size offset)
            : base(actual)
        {
            _offset = offset;
            //StopByObjectId(2188);
        }

        internal ReplaceRelRefArg(CodeBase actualArg)
            : this(actualArg, Size.Create(0)) { }

        private RefAlignParam RefAlignParam { get { return ActualArg.RefAlignParam; } }

        protected override CodeBase Actual
        {
            get
            {
                if (_offset.IsZero)
                    return ActualArg;
                return ActualArg.CreateRefPlus(RefAlignParam, Offset, "ReplaceRelRefArg.Actual");
            }
        }

        private Size Offset { get { return _offset; } }

        protected override Visitor<CodeBase> After(Size size)
        {
            return new ReplaceRelRefArg(ActualArg, Offset + size);
        }
    }

}