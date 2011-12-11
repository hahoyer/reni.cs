// 
//     Project Reni2
//     Copyright (C) 2011 - 2011 Harald Hoyer
// 
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//     
//     Comments, bugs and suggestions to hahoyer at yahoo.de

using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using HWClassLibrary.Helper;
using Reni.Basics;

namespace Reni.Type
{
    abstract class FunctionalFeature : ReniObject, IFunctionalFeature
    {
        static int _nextObjectId;
        readonly DictionaryEx<RefAlignParam, TypeBase> _functionalTypesCache;

        protected FunctionalFeature()
            : base(_nextObjectId++) { _functionalTypesCache = new DictionaryEx<RefAlignParam, TypeBase>(refAlignParam => new FunctionalFeatureType<IFunctionalFeature>(this, refAlignParam)); }

        internal TypeBase UniqueFunctionalType(RefAlignParam refAlignParam) { return _functionalTypesCache.Find(refAlignParam); }

        string IDumpShortProvider.DumpShort() { return DumpShort(); }

        Result IFunctionalFeature.ApplyResult(Category category, Result argsResult, RefAlignParam refAlignParam)
        {
            var trace = ObjectId == -10 && category.HasCode;
            StartMethodDump(trace, category, argsResult, refAlignParam);
            try
            {
                var applyResult = ObtainApplyResult(category, argsResult.Type, refAlignParam);
                if(!category.HasCode && !category.HasArgs)
                    return ReturnMethodDump(applyResult);

                Dump("applyResult", applyResult);
                BreakExecution();
                var replaceArgResult = applyResult.ReplaceArg(argsResult);
                if(ObjectType.IsDataLess)
                    return ReturnMethodDump(replaceArgResult);

                Dump("replaceArgResult", replaceArgResult);
                BreakExecution();
                var result = ReplaceObjectReferenceByArg(replaceArgResult, refAlignParam);
                return ReturnMethodDump(result, true);
            }
            finally
            {
                EndMethodDump();
            }
        }
        public TypeBase ObjectReference(RefAlignParam refAlignParam) { return ObjectType.UniqueAutomaticReference(refAlignParam); }

        protected abstract Result ObtainApplyResult(Category category, TypeBase argsType, RefAlignParam refAlignParam);
        protected abstract TypeBase ObjectType { get; }
        protected abstract Result ReplaceObjectReferenceByArg(Result result, RefAlignParam refAlignParam);
    }
}