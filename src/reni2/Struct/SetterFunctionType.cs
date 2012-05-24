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
using Reni.Context;
using Reni.Syntax;
using Reni.Type;

namespace Reni.Struct
{
    sealed class SetterFunctionType : FunctionInstanceType, IFunctionalFeature
    {
        readonly FunctionId _functionId;
        public SetterFunctionType(FunctionType parent, int index, CompileSyntax body)
            : base(parent, body)
        {
            _functionId = FunctionId
                .Setter(index);
        }

        protected override FunctionId FunctionId { get { return _functionId; } }
        protected override ContextBase Context { get { return Parent.SetterContext; } }
        protected override Size FrameSize { get { return base.FrameSize + RefAlignParam.RefSize; } }
        protected override TypeBase CallObjectType { get { return Parent.Pair(Parent.ValueType.UniqueReference(RefAlignParam).Type); } }

        Result IFunctionalFeature.ApplyResult(Category category, TypeBase argsType) { return AssignmentResult(category, argsType, Parent); }
        bool IFunctionalFeature.IsImplicit { get { return Parent.IsImplicit; } }
        IReferenceInCode IFunctionalFeature.ObjectReference { get { return Parent; } }
    }
}