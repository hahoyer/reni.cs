using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Basics;
using Reni.Context;

namespace Reni
{
    [Serializable]
    internal sealed class PostProcessorForResult : ReniObject
    {
        private readonly Result _result;

        internal PostProcessorForResult(Result result) { _result = result; }

        internal Result FunctionResult(Category category, RefAlignParam refAlignParam)
        {
            return _result
                .AutomaticDereference()
                .Align(refAlignParam.AlignBits)
                .LocalBlock(category, refAlignParam);
        }
    }
}