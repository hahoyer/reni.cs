#region Copyright (C) 2012

//     Project Reni2
//     Copyright (C) 2012 - 2012 Harald Hoyer
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

#endregion

using System.Linq;
using System.Collections.Generic;
using System;
using HWClassLibrary.Debug;
using Reni.Basics;
using Reni.Code;
using Reni.Feature;
using Reni.Struct;

namespace Reni.Type
{
    sealed class AssignmentFeature : ReniObject, IFunctionFeature, ISuffixFeature
    {
        readonly ISetterTargetType _target;
        public AssignmentFeature(ISetterTargetType target) { _target = target; }

        Result IFunctionFeature.ApplyResult(Category category, TypeBase argsType)
        {
            var trace = ObjectId == -1;
            StartMethodDump(trace, category,argsType);
            try
            {
                BreakExecution();
                var sourceResult = argsType
                    .Conversion(category, _target.ValueType.UniqueReference.Type());
                Dump("sourceResult", sourceResult); 
                BreakExecution();

                var destinationResult = _target
                    .DestinationResult(category.Typed)
                    .ReplaceArg(_target.Type.Result(category.Typed, _target));
                Dump("destinationResult", destinationResult); 
                BreakExecution();

                var resultForArg = destinationResult + sourceResult;
                Dump("resultForArg", resultForArg); 

                var result = _target.Result(category);
                Dump("result", result);
                BreakExecution();

                return ReturnMethodDump(result.ReplaceArg(resultForArg),true);

            }
            finally
            {
                EndMethodDump();
            }
        }

        IMetaFunctionFeature IFeature.MetaFunction { get { return null; } }
        IFunctionFeature IFeature.Function { get { return this; } }
        ISimpleFeature IFeature.Simple { get { return null; } }
        
        bool IFunctionFeature.IsImplicit { get { return false; } }
        IContextReference IFunctionFeature.ObjectReference { get { return _target; } }
    }
}