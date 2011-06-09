using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Reni.Context
{
    internal sealed class FunctionContext
    {
        private readonly Function _function;
        private readonly ContextBase _parent;

        public FunctionContext(Function function, ContextBase parent)
        {
            _function = function;
            _parent = parent;
        }

        internal Result CreateArgsReferenceResult(Category category) { return _function.ArgsType.ReferenceInCode(_parent, category); }

    }
}