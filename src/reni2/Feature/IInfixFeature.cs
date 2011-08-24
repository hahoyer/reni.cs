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

using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Basics;
using Reni.Type;

namespace Reni.Feature
{
    internal interface ISearchPath<out TOutType, in TInType>
    {
        TOutType Convert(TInType type);
    }

    internal interface IFeature
    {
        Result ObtainResult(Category category, RefAlignParam refAlignParam);
        TypeBase ObjectType { get; }
    }

    internal interface IPrefixFeature
    {
        IFeature Feature { get; }
    }


    internal interface IContextFeature
    {
        Result ObtainResult(Category category);
    }

    internal sealed class Feature : ReniObject, IFeature
    {
        [EnableDump]
        private readonly Func<Category, RefAlignParam, Result> _function;
        private static int _nextObjectId;

        public Feature(Func<Category, RefAlignParam, Result> function)
            : base(_nextObjectId++)
        {
            _function = function;
            Tracer.Assert(_function.Target is TypeBase);
        }

        Result IFeature.ObtainResult(Category category, RefAlignParam refAlignParam) { return _function(category, refAlignParam); }
        TypeBase IFeature.ObjectType { get { return (TypeBase) _function.Target; } }
    }

    internal sealed class PrefixFeature : ReniObject, IPrefixFeature, IFeature
    {
        [EnableDump]
        private readonly Func<Category, RefAlignParam, Result> _function;
        private static int _nextObjectId;

        public PrefixFeature(Func<Category, RefAlignParam, Result> function)
            : base(_nextObjectId++)
        {
            _function = function;
            Tracer.Assert(_function.Target is TypeBase);
        }

        Result IFeature.ObtainResult(Category category, RefAlignParam refAlignParam) { return _function(category, refAlignParam); }
        TypeBase IFeature.ObjectType { get { return (TypeBase) _function.Target; } }
        IFeature IPrefixFeature.Feature { get { return this; } }
    }

    internal static class FeatureExtender
    {
        internal static TypeBase TypeOfArgInObtainResult(this IFeature feature, RefAlignParam refAlignParam)
        {
            return feature
                .ObjectType
                .SmartReference(refAlignParam);
        }

        internal static IFeature ConvertToFeature(this object feature)
        {
            if(feature is IPrefixFeature)
                return ((IPrefixFeature) feature).Feature;
            return (IFeature) feature;
        }
    }
}