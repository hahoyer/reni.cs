using HWClassLibrary.Debug;
using Reni.Context;

namespace Reni.Code.ReplaceVisitor
{
    /// <summary>
    /// Handle argument replaces of relative args
    /// </summary>
    public sealed class ReplaceRelRefArg : ReplaceArg
    {
        private readonly RefAlignParam _refAlignParam;

        [DumpData(true)] 
        private readonly Size _offset;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ReplaceRelRefArg"/> class.
        /// </summary>
        /// <param name="actual">The actual.</param>
        /// <param name="refAlignParam">The ref align param.</param>
        /// <param name="offset">The offset.</param>
        /// created 28.09.2006 23:03
        ReplaceRelRefArg(Code.Base actual, RefAlignParam refAlignParam, Size offset)
            : base(actual)
        {
            _refAlignParam = refAlignParam;
            _offset = offset;
            //StopByObjectId(2188);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ReplaceRelRefArg"/> class.
        /// </summary>
        /// <param name="actualArg">The actual.</param>
        /// <param name="refAlignParam">The ref align param.</param>
        /// created 28.09.2006 22:52
        /// created 18.10.2006 00:21
        public ReplaceRelRefArg(Code.Base actualArg, RefAlignParam refAlignParam)
            : this(actualArg, refAlignParam, Size.Create(0))
        {
        }

        /// <summary>
        /// Gets the ref align param.
        /// </summary>
        /// <value>The ref align param.</value>
        /// created 19.10.2006 19:35
        public RefAlignParam RefAlignParam { get { return _refAlignParam; } }
        /// <summary>
        /// Gets the actual.
        /// </summary>
        /// <value>The actual.</value>
        /// created 28.09.2006 22:46
        /// created 28.09.2006 22:58
        public override Code.Base Actual
        {
            get
            {
                if (_offset.IsZero)
                    return ActualArg;
                return ActualArg.CreateRefPlus(RefAlignParam, Offset);
            }
        }

        /// <summary>
        /// Gets the offset.
        /// </summary>
        /// <value>The offset.</value>
        /// created 18.10.2006 00:35
        public Size Offset { get { return _offset; } }

        /// <summary>
        /// Afters the specified size.
        /// </summary>
        /// <param name="size">The size.</param>
        /// <returns></returns>
        /// created 15.10.2006 18:32
        public override Visitor<Code.Base> After(Size size)
        {
            if (size.IsZero)
                return this;
            return new ReplaceRelRefArg(ActualArg,RefAlignParam, Offset + size);
        }
    }
}