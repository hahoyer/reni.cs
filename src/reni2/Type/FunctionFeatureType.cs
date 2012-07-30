#region Copyright (C) 2012

//     Project Reni2
//     Copyright (C) 2011 - 2012 Harald Hoyer
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

using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;

namespace Reni.Type
{
    sealed class FunctionFeatureType : TypeBase, IFunctionFeature
    {
        readonly FunctionalFeature _functionalFeature;

        internal FunctionFeatureType(FunctionalFeature functionalFeature) { _functionalFeature = functionalFeature; }

        [DisableDump]
        internal override bool IsDataLess { get { return false; } }
        [DisableDump]
        bool IFunctionFeature.IsImplicit { get { return false; } }
        [DisableDump]
        internal override bool IsLambda { get { return true; } }
        [DisableDump]
        IContextReference IFunctionFeature.ObjectReference { get { return _functionalFeature.ObjectReference; } }
        [DisableDump]
        internal override Root RootContext { get { return _functionalFeature.RootContext; } }
        protected override Size GetSize() { return Root.DefaultRefAlignParam.RefSize; }
        internal override string DumpShort() { return base.DumpShort() + "(" + _functionalFeature.DumpShort() + ")"; }

        Result IFunctionFeature.ApplyResult(Category category, TypeBase argsType)
        {
            return _functionalFeature
                .ApplyResult(category, argsType);
        }
    }
}