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

namespace Reni.Type
{
    internal abstract class FunctionalFeature : ReniObject, IFunctionalFeature
    {
        protected abstract Result Apply(Category category, TypeBase argsType, RefAlignParam refAlignParam);
        protected abstract TypeBase ObjectType { get; }

        Result IFunctionalFeature.Apply(Category category, Result operationResult, Result argsResult, RefAlignParam refAlignParam)
        {
            var trace = ObjectId == -98;
            StartMethodDump(trace, category);
            try
            {
                BreakExecution();
                var applyResult = Apply(category, argsResult.Type, refAlignParam)
                    .ReplaceArg(argsResult)
                    .ReplaceObjectRefByArg(refAlignParam, ObjectType);

                if(!category.HasCode && !category.HasRefs || ObjectType.Size.IsZero)
                    return ReturnMethodDump(applyResult);

                Dump("applyResult", applyResult);
                BreakExecution();
                var objectResult = ObjectType
                    .UniqueAutomaticReference(refAlignParam)
                    .Result(category.Typed, operationResult);
                var result = applyResult.ReplaceArg(objectResult);
                return ReturnMethodDump(result, true);
            }
            finally
            {
                EndMethodDump();
            }
        }

        string IDumpShortProvider.DumpShort() { return DumpShort(); }
    }
}