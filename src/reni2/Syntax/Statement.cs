using System;
using System.Collections.Generic;
using HWClassLibrary.Debug;
using Reni.Context;
using Reni.Parser;

namespace Reni.Syntax
{
    /// <summary>
    /// Statement inside of a struct, context free version
    /// </summary>
    sealed internal class Statement : Base
    {
        private readonly List<MemberElem> _chain;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="chain">The chain.</param>
        /// <param name="tail">The tail.</param>
        Statement(List<MemberElem> chain, MemberElem tail)
        {
            _chain = new List<MemberElem>();
            _chain.AddRange(chain);
            _chain.Add(tail);
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="head">The head.</param>
        /// <param name="tail">The tail.</param>
        public Statement(MemberElem head, MemberElem tail)
        {
            _chain = new List<MemberElem>();
            _chain.Add(head);
            _chain.Add(tail);
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="head">The head.</param>
        public Statement(MemberElem head)
        {
            _chain = new List<MemberElem>();
            _chain.Add(head);
        }

        /// <summary>
        /// 
        /// </summary>
        public List<MemberElem> Chain { get { return _chain; } }

        /// <summary>
        /// Visitor function 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        override public Result VirtVisit(Context.Base context, Category category)
        {
            bool trace = ObjectId == -69 && context.ObjectId == 4 && category.HasRefs;
            StartMethodDumpWithBreak(trace, context,category);
            if (Chain.Count == 0)
                NotImplementedMethod(context, category);

            Category internalCategory = category|Category.Type;
            if (category.HasCode) 
                internalCategory |= Category.Refs;
            Result intermediateResult = Reni.Type.Base.CreateVoidResult(internalCategory);

            Result result = context
                .VisitFirstChainElement(internalCategory, Chain[0])
                .Align(context.RefAlignParam.AlignBits);
            for (int i = 1; i < Chain.Count; i++)
            {
                Tracer.ConditionalBreak(trace, result.Dump());
                if (!result.Type.IsRef)
                    intermediateResult = intermediateResult.SafeList(result, internalCategory);
                if (result.IsPending)
                    return result;

                result = context
                    .VisitNextChainElement(internalCategory, Chain[i], result)
                    .Align(context.RefAlignParam.AlignBits);
                if(internalCategory.HasRefs)
                foreach (Context.Base referencedContext in result.Refs.Data)
                {
                    if (referencedContext.IsChildOf(context))
                    {
                        Code.Base replaceContextCode = intermediateResult.Type.CreateRefCodeForContext(referencedContext);
                        Tracer.Assert(replaceContextCode != null);
                        result = result.ReplaceRelativeContextRef(referencedContext, replaceContextCode);
                    }
                }
            }

            if (result.IsPending)
                return result;
            Tracer.Assert(result != null);
            Result statementResult = result
                .Type
                .Dereference(result)
                .Align(context.RefAlignParam.AlignBits)
                .CreateStatement(category, intermediateResult);
            return ReturnMethodDumpWithBreak(trace, statementResult);
        }

        /// <summary>
        /// Creates the definable syntax.
        /// </summary>
        /// <param name="defineableToken">The defineable token.</param>
        /// <param name="right">The right.</param>
        /// <returns></returns>
        /// created 01.04.2007 23:06 on SAPHIRE by HH
        internal override Base CreateDefinableSyntax(DefineableToken defineableToken, Base right)
        {
            return new Statement(_chain,new MemberElem(defineableToken,right));
        }

        /// <summary>
        /// Dumps the short.
        /// </summary>
        /// <returns></returns>
        /// created 07.05.2007 22:09 on HAHOYER-DELL by hh
        /// created 07.05.2007 22:10 on HAHOYER-DELL by hh
        internal override string DumpShort()
        {
            string result = "";
            for (int i = 0; i < _chain.Count; i++)
            {
                if(i>0)
                    result += " ";
                result += _chain[i].DumpShort();
            }
            return result;
        }
    }
}
