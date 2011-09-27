using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Basics;
using Reni.Context;
using Reni.Type;

namespace Reni.Code.ReplaceVisitor
{
    internal sealed class ReplaceRelRefArg : ReplaceArg
    {
        [EnableDump]
        private readonly Size _offset;

        private ReplaceRelRefArg(Result actualArg, Size offset)
            : base(actualArg)
        {
            _offset = offset;
            StopByObjectId(-22);
        }

        internal ReplaceRelRefArg(Result actualArg)
            : this(actualArg, Size.Create(0)) { }

        private RefAlignParam RefAlignParam { get { return ActualArg.Code.RefAlignParam; } }

        protected override CodeBase Actual
        {
            get
            {
                if(_offset.IsZero)
                    return ActualArg.Code;
                return ActualArg.Code.AddToReference(RefAlignParam, Offset);
            }
        }

        private Size Offset { get { return _offset; } }

        protected override Visitor<CodeBase> After(Size size) { return new ReplaceRelRefArg(ActualArg, Offset + size); }
    }
}