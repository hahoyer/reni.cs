using HWClassLibrary.Debug;
using Reni.Context;
using Reni.Feature;
using Reni.Syntax;
using Reni.Type;

namespace Reni.Parser.TokenClass
{
    internal abstract class SequenceOfBitOperation : Defineable, IFeature, IPrefixFeature
    {
        [DumpExcept(false)]
        internal protected virtual bool IsBitSequencePrefixOperation { get { return false; } }

        public Result VisitApply(ContextBase context, Category category, SyntaxBase args, Ref objectType)
        {
            var elementType = objectType.SequenceElementType;
            var objResult = objectType.VisitAsSequence(category, elementType);
            var argResult = args.VisitAsSequence(context, category | Category.Type, elementType);
            var result = new Result();
            if(category.HasSize || category.HasType || category.HasCode)
            {
                var objBitCount = objectType.UnrefSize.ToInt();
                var argBitCount = argResult.Type.UnrefSize.ToInt();
                var type =
                    elementType
                        .SequenceOperationResultType(this, objBitCount, argBitCount)
                        .CreateAlign(context.RefAlignParam.AlignBits);
                if(category.HasSize)
                    result.Size = type.Size;
                if(category.HasType)
                    result.Type = type;
                if(category.HasCode)
                    result.Code = elementType.CreateSequenceOperation(this, objResult, type.Size,
                        argResult);
            }
            if(category.HasRefs)
                result.Refs = objResult.Refs.Pair(argResult.Refs);
            return result;
        }

        public Result VisitApply(Category category, Result argResult)
        {
            var elementType = argResult.Type.SequenceElementType;
            var objResult = argResult.Type.VisitAsSequence(category, elementType).UseWithArg(argResult);
            var result = new Result();
            if(category.HasSize || category.HasType || category.HasCode)
            {
                if(category.HasSize)
                    result.Size = objResult.Size;
                if(category.HasType)
                    result.Type = objResult.Type;
                if(category.HasCode)
                    result.Code = elementType.CreateSequenceOperation(this, objResult);
            }
            if(category.HasRefs)
                result.Refs = objResult.Refs;
            return result;
        }

        internal override sealed SearchResult<IFeature> SearchFromSequenceOfBit()
        {
            return SearchResult<IFeature>.Success(this, this);
        }

        internal override sealed SearchResult<IPrefixFeature> SearchPrefixFromSequenceOfBit()
        {
            if(IsBitSequencePrefixOperation)
                return SearchResult<IPrefixFeature>.Success(this, this);
            return base.SearchPrefixFromSequenceOfBit();
        }
    }
}