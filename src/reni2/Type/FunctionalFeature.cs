//     Compiler for programming language "Reni"
//     Copyright (C) 2011 Harald Hoyer
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
using Reni.Basics;
using Reni.Struct;

namespace Reni.Type
{
    internal abstract class FunctionalFeature : ReniObject, IFunctionalFeature
    {
        private static int _nextObjectId;
        protected FunctionalFeature()
            : base(_nextObjectId++) {}
        protected abstract Result ObtainApplyResult(Category category, TypeBase argsType, RefAlignParam refAlignParam);
        protected abstract TypeBase ObjectType { get; }

        Result IFunctionalFeature.ObtainApplyResult(Category category, Result operationResult, Result argsResult, RefAlignParam refAlignParam)
        {
            var trace = ObjectId == -10 && category.HasCode;
            StartMethodDump(trace, category,operationResult,argsResult,refAlignParam);
            try
            {
                BreakExecution();
                var applyResult = ObtainApplyResult(category, argsResult.Type, refAlignParam);
                if(!category.HasCode && !category.HasRefs)
                    return ReturnMethodDump(applyResult);

                Dump("applyResult", applyResult);
                BreakExecution();
                var replaceArgResult = applyResult.ReplaceArg(argsResult);
                if (ObjectType.Size.IsZero)
                    return ReturnMethodDump(replaceArgResult);

                var replaceObjectResult = ReplaceObjectReferenceByArg(replaceArgResult, refAlignParam);
                Dump("replaceObjectResult", replaceObjectResult);
                BreakExecution();
                var objectResult = ObjectType
                    .ForceReference(refAlignParam)
                    .Result(category.Typed, operationResult);
                Dump("objectResult", objectResult);
                var result = replaceObjectResult.ReplaceArg(objectResult);
                return ReturnMethodDump(result, true);
            }
            finally
            {
                EndMethodDump();
            }
        }

        protected virtual Result ReplaceObjectReferenceByArg(Result result, RefAlignParam refAlignParam)
        {
            NotImplementedMethod(result,refAlignParam);
            return result;
        }

        private Structure FindAccessPoint(Result result)
        {
            if (!result.HasType)
                return null;
            var accessType = result.Type as AccessType;
            if (accessType == null)
                return null;

            return accessType.AccessPoint;
        }

        string IDumpShortProvider.DumpShort() { return DumpShort(); }
    }
}