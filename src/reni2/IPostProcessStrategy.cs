using System;
using HWClassLibrary.Debug;
using Reni.Code;
using Reni.Context;
using Reni.Type;

namespace Reni
{
    [Serializable]
    internal class PostProcessorForResult : ReniObject
    {
        private readonly Result _result;

        internal PostProcessorForResult(Result result) { _result = result; }

        internal Result InternalResultForStruct(Category category, int alignBits)
        {
            return _result
                .AutomaticDereference()
                .Align(alignBits)
                .CreateStatement(category);
        }

        internal Result FunctionResult(int alignBits)
        {
            return _result
                .AutomaticDereference()
                .Align(alignBits);
        }

        internal Result ArgsResult(int alignBits)
        {
            return _result
                .AutomaticDereference()
                .Align(alignBits);
        }
    }

    [Serializable]
    internal class PostProcessorForType : ReniObject
    {
        private readonly TypeBase _typeBase;

        internal PostProcessorForType(TypeBase typeBase) { _typeBase = typeBase; }

        internal Result AccessResultForStruct(Category category, RefAlignParam refAlignParam, Func<CodeBase> getCode)
        {
            return _typeBase
                .AutomaticDereference()
                .CreateAssignableRefResult(category, refAlignParam, getCode);
        }

        [Obsolete]
        internal Result PostProcess_old(AutomaticRef visitedType, Result result)
        {
            if(_typeBase == visitedType.Target)
                return result.UseWithArg(visitedType.CreateDereferencedArgResult(result.Complete));
            NotImplementedMethod(visitedType, result);
            return null;
        }

        [Obsolete]
        internal Result PostProcess_oldOfAutomaticRef(AutomaticRef visitedType, Result result)
        {
            if(_typeBase == visitedType)
                return result;
            return PostProcess_old(visitedType, result);
        }
    }
}