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

namespace Reni.Feature
{
    abstract class SimpleBase : ReniObject, ISimpleFeature
    {
        [EnableDump]
        Func<Category, Result> _function;
        static int _nextObjectId;
        protected SimpleBase(Func<Category, Result> function)
            : base(_nextObjectId++) { _function = function; }
        Result ISimpleFeature.Result(Category category) { return _function(category); }
    }

    sealed class Simple<TType>
        : ReniObject
          , ISearchPath<ISuffixFeature, TType>
          , ISearchPath<IPrefixFeature, TType>
    {
        readonly Func<Category, TType, Result> _function;

        public Simple(Func<Category, TType, Result> function) { _function = function; }

        ISuffixFeature ISearchPath<ISuffixFeature, TType>.Convert(TType type) { return new ConvertedSimple<TType>(type, _function); }
        IPrefixFeature ISearchPath<IPrefixFeature, TType>.Convert(TType type) { return new ConvertedSimple<TType>(type, _function); }
    }

    sealed class ConvertedSimple<TType>
        : ReniObject, ISimpleFeature, ISuffixFeature
          , IPrefixFeature
    {
        readonly TType _type;
        readonly Func<Category, TType, Result> _function;
        public ConvertedSimple(TType type, Func<Category, TType, Result> function)
        {
            _type = type;
            _function = function;
        }

        IMetaFunctionFeature IFeature.MetaFunction { get { return null; } }
        IFunctionFeature IFeature.Function { get { return null; } }
        ISimpleFeature IFeature.Simple { get { return this; } }
        Result ISimpleFeature.Result(Category category) { return _function(category, _type); }
    }

    sealed class Simple : SimpleBase, IPrefixFeature, ISuffixFeature
    {
        public Simple(Func<Category, Result> function)
            : base(function) { }

        IMetaFunctionFeature IFeature.MetaFunction { get { return null; } }
        IFunctionFeature IFeature.Function { get { return null; } }
        ISimpleFeature IFeature.Simple { get { return this; } }
    }
}