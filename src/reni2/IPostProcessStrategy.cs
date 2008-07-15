using System;
using Reni.Context;
using Reni.Type;

namespace Reni
{
    internal class PostProcessorForResult : ReniObject
    {
        private readonly Result _result;

        internal PostProcessorForResult(Result result)
        {
            _result = result;
        }

        internal Result InternalResultForStruct(int AlignBits)
        {
            return _result.AutomaticDereference().Align(AlignBits);
        }
    }

    internal class PostProcessorForType: ReniObject
    {
        private readonly TypeBase _typeBase;

        internal PostProcessorForType(TypeBase typeBase)
        {
            _typeBase = typeBase;
        }

        internal Result AccessResultForStruct(Category category, RefAlignParam refAlignParam, Result.GetCode getCode)
        {
            return _typeBase.AutomaticDereference().CreateAssignableRefResult(category, refAlignParam, getCode);
        }

        [Obsolete]
        internal Result PostProcess_old(AutomaticRef visitedType, Result result)
        {
            if (_typeBase == visitedType.Target)
                return result.UseWithArg(visitedType.CreateDereferencedArgResult(result.Complete));
            NotImplementedMethod(visitedType, result);
            return null;
        }
        [Obsolete]
        internal Result PostProcess_oldOfAutomaticRef(AutomaticRef visitedType, Result result)
        {
            if (_typeBase == visitedType)
                return result;
            return PostProcess_old(visitedType, result);
        }

    
    }

}