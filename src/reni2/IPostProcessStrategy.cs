using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Context;

namespace Reni
{
    [Serializable]
    internal class PostProcessorForResult : ReniObject
    {
        private readonly Result _result;

        internal PostProcessorForResult(Result result) { _result = result; }

        internal Result InternalResultForStruct(Category category, RefAlignParam refAlignParam)
        {
            return _result
                .AutomaticDereference()
                .Align(refAlignParam.AlignBits)
                .LocalBlock(category, refAlignParam);
        }

        internal Result FunctionResult(Category category, RefAlignParam refAlignParam)
        {
            return _result
                .AutomaticDereference()
                .Align(refAlignParam.AlignBits)
                .LocalBlock(category, refAlignParam);
        }

        internal Result ArgsResult(int alignBits)
        {
            return _result
                .AutomaticDereference()
                .Align(alignBits);
        }
    }
}