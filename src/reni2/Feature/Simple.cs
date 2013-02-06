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
using Reni.Sequence;

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

        public bool IsEqual(SimpleBase other)
        {
            if(_function == other._function)
                return true;
            if(_function.Method != other._function.Method)
                return false;

            if(_function.Target == other._function.Target)
                return true;
            if(_function.Target.GetType() != other._function.Target.GetType())
                return false;

            var simple = _function.Target as Simple<SequenceType>;
            if(simple != null)
                return simple.IsEqual((Simple<SequenceType>) other._function.Target);

            Tracer.ConditionalBreak(true);

            return false;
        }
    }

    sealed class Simple<TType>
        : ReniObject
          , ISearchPath<ISuffixFeature, TType>
          , ISearchPath<IPrefixFeature, TType>
    {
        readonly Func<Category, TType, Result> _function;

        public Simple(Func<Category, TType, Result> function) { _function = function; }

        ISuffixFeature ISearchPath<ISuffixFeature, TType>.Convert(TType type) { return Extension.Feature(category => _function(category, type)); }
        IPrefixFeature ISearchPath<IPrefixFeature, TType>.Convert(TType type) { return Extension.Feature(category => _function(category, type)); }

        public bool IsEqual(Simple<TType> other)
        {
            if (_function == other._function)
                return true;
            if (_function.Method != other._function.Method)
                return false;

            if (_function.Target == other._function.Target)
                return true;
            if (_function.Target.GetType() != other._function.Target.GetType())
                return false;

            Tracer.ConditionalBreak(true);

            return false;
        }
    }

    sealed class Simple : SimpleBase, IPrefixFeature, ISuffixFeature
    {
        public Simple(Func<Category, Result> function)
            : base(function) { }

        IMetaFunctionFeature IFeature.MetaFunction { get { return null; } }
        IFunctionFeature IFeature.Function { get { return null; } }
        ISimpleFeature IFeature.Simple { get { return this; } }
    }

    sealed class Simple<TType1, TType2>
        : ReniObject
          , ISearchPath<ISearchPath<ISuffixFeature, TType1>, TType2>
    {
        readonly Func<Category, TType1, TType2, Result> _function;
        internal Simple(Func<Category, TType1, TType2, Result> function) { _function = function; }
        ISearchPath<ISuffixFeature, TType1>
            ISearchPath<ISearchPath<ISuffixFeature, TType1>, TType2>
                .Convert(TType2 type) { return new Simple<TType1>((category,type1) => _function(category, type1, type)); }
    }
}