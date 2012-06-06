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
using Reni.Type;

namespace Reni.Feature
{
    abstract class FunctionBase : ReniObject, IFunctionFeature, IContextReference
    {
        [EnableDump]
        readonly Func<Category, IContextReference, TypeBase, Result> _function;
        static int _nextObjectId;

        protected FunctionBase(Func<Category, IContextReference, TypeBase, Result> function)
            : base(_nextObjectId++)
        {
            _function = function;
            Tracer.Assert(_function.Target is IContextReference);
        }

        Result IFunctionFeature.ApplyResult(Category category, TypeBase argsType) { return _function(category, this, argsType); }
        bool IFunctionFeature.IsImplicit { get { return false; } }
        IContextReference IFunctionFeature.ObjectReference { get { return this; } }
        Size IContextReference.Size { get { return Root.DefaultRefAlignParam.RefSize; } }
    }

    sealed class Function : FunctionBase, ISuffixFeature
    {
        public Function(Func<Category, IContextReference, TypeBase, Result> function)
            : base(function) { }

        IMetaFunctionFeature IFeature.MetaFunction { get { return null; } }
        IFunctionFeature IFeature.Function { get { return this; } }
        ISimpleFeature IFeature.Simple { get { return null; } }
    }

    sealed class Function<TType>
        : FunctionBase
          , ISearchPath<ISuffixFeature, TType>
          , ISuffixFeature
    {
        public Function(Func<Category, IContextReference, TypeBase, Result> function)
            : base(function) { }

        ISuffixFeature ISearchPath<ISuffixFeature, TType>.Convert(TType type) { return this; }

        IMetaFunctionFeature IFeature.MetaFunction { get { return null; } }
        IFunctionFeature IFeature.Function { get { return this; } }
        ISimpleFeature IFeature.Simple { get { return null; } }
    }
}
