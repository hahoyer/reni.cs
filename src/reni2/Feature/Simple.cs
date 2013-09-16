#region Copyright (C) 2013

//     Project Reni2
//     Copyright (C) 2012 - 2013 Harald Hoyer
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
using hw.Debug;
using Reni.Basics;
using Reni.Type;

namespace Reni.Feature
{
    abstract class SimpleBase : DumpableObject, ISimpleFeature
    {
        [EnableDump]
        Func<Category, Result> _function;
        static int _nextObjectId;
        protected SimpleBase(Func<Category, Result> function)
            : base(_nextObjectId++) { _function = function; }
        Result ISimpleFeature.Result(Category category) { return _function(category); }
    }

    sealed class Simple : SimpleBase, IFeatureImplementation
    {
        public Simple(Func<Category, Result> function)
            : base(function) { }

        IMetaFunctionFeature IFeatureImplementation.MetaFunction { get { return null; } }
        IFunctionFeature IFeatureImplementation.Function { get { return null; } }
        ISimpleFeature IFeatureImplementation.Simple { get { return this; } }
    }

    sealed class Simple<TType>
        : DumpableObject
            , IPath<IFeatureImplementation, TType>
        where TType : TypeBase
    {
        readonly Func<Category, TType, Result> _function;

        public Simple(Func<Category, TType, Result> function) { _function = function; }
        IFeatureImplementation IPath<IFeatureImplementation, TType>.Convert(TType provider) { return new Simple((category) => _function(category, provider)); }
    }

    sealed class Simple<TType1, TType2>
        : DumpableObject
            , IPath<IPath<IFeatureImplementation, TType1>, TType2>
        where TType2 : TypeBase
        where TType1 : TypeBase
    {
        readonly Func<Category, TType1, TType2, Result> _function;
        internal Simple(Func<Category, TType1, TType2, Result> function) { _function = function; }
        IPath<IFeatureImplementation, TType1> IPath<IPath<IFeatureImplementation, TType1>, TType2>.Convert(TType2 provider) { return new Simple<TType1>((category, type1) => _function(category, type1, provider)); }
    }
}