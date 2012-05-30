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
using Reni.Type;

namespace Reni.Struct
{
    sealed class SetterType : TypeBase, IFunctionalFeature
    {
        [EnableDump]
        readonly ISetterTargetType _target;

        public SetterType(ISetterTargetType target) { _target = target; }
        [DisableDump]
        internal override bool IsDataLess { get { return false; } }
        protected override Size GetSize() { return _target.RefSize; }
        [DisableDump]
        IReferenceInCode IFunctionalFeature.ObjectReference { get { return _target.ObjectReference; } }
        [DisableDump]
        bool IFunctionalFeature.IsImplicit
        {
            get
            {
                NotImplementedMethod();
                return false;
            }
        }

        internal static Result AssignmentResult(Category category, TypeBase argsType, ISetterTargetType target)
        {
            var sourceResult = argsType
                .Conversion(category, target.ValueType.UniqueReference(target.RefAlignParam).Type);
            var destinationResult = target
                .DestinationResult(category.Typed)
                .ReplaceArg(target.Type.Result(category.Typed, target));
            var resultForArg = destinationResult + sourceResult;
            return target
                .Result(category)
                .ReplaceArg(resultForArg);
        }

        Result IFunctionalFeature.ApplyResult(Category category, TypeBase argsType)
        {
            var trace = ObjectId == 16 && category.HasCode;
            StartMethodDump(trace, category, argsType);
            try
            {
                var valueType = _target.ValueType ?? argsType;

                Dump("valueType", valueType);
                BreakExecution();

                var rawResult = _target.Result(category);

                Dump("rawResult", rawResult);

                var sourceResult = argsType.Conversion(category, valueType.UniqueReference(_target.RefAlignParam).Type);
                Dump("sourceResult", sourceResult);
                var destinationResult = _target.DestinationResult(category.Typed);
                Dump("destinationResult", destinationResult);
                var resultForArg = destinationResult + sourceResult;
                Dump("resultForArg", resultForArg);

                BreakExecution();

                var result = rawResult.ReplaceArg(resultForArg);
                return ReturnMethodDump(result, true);
            }
            finally
            {
                EndMethodDump();
            }
        }
        internal override void Search(SearchVisitor searchVisitor) { NotImplementedMethod(); }
        internal Result AssignmentFeatureResult(Category category) { return new Result(category, getType: () => this, getCode: () => _target.Type.ArgCode()); }
    }

    interface ISetterTargetType : IReferenceInCode
    {
        TypeBase ValueType { get; }
        IReferenceInCode ObjectReference { get; }
        TypeBase Type { get; }
        Result Result(Category category);
        Result DestinationResult(Category category);
        SetterType SetterType { get; }
    }
}