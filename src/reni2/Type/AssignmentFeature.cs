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

namespace Reni.Type
{
    sealed class AssignmentFeature : ReniObject, IFunctionFeature, ISuffixFeature
    {
        static int _nextObjectId;
        readonly SetterTargetType _target;

        public AssignmentFeature(SetterTargetType target)
            : base(_nextObjectId++) { _target = target; }

        Result IFunctionFeature.ApplyResult(Category category, TypeBase argsType)
        {
            if (category == Category.Type)
                return _target.RootContext.VoidResult(category);

            var trace = ObjectId == -1;
            StartMethodDump(trace, category, argsType);
            try
            {
                BreakExecution();
                var sourceResult = argsType
                    .Conversion(category.Typed, _target.ValueType)
                    .LocalPointerKindResult();
                Dump("sourceResult", sourceResult);
                BreakExecution();

                var destinationResult = _target
                    .DestinationResult(category.Typed)
                    .ReplaceArg(_target.Result(category.Typed, _target));
                Dump("destinationResult", destinationResult);
                BreakExecution();

                var resultForArg = destinationResult + sourceResult;
                Dump("resultForArg", resultForArg);

                var result = _target.RootContext.VoidType.Result(category, _target.SetterResult);
                Dump("result", result);
                BreakExecution();

                return ReturnMethodDump(result.ReplaceArg(resultForArg));
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