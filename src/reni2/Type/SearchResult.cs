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

using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using Reni.Basics;
using Reni.Feature;

namespace Reni.Type
{
    sealed class SearchResult<TFeature> : ReniObject
        where TFeature : class
    {
        internal readonly TFeature Feature;
        readonly IFoundItem[] _foundPath;
        internal SearchResult(TFeature feature, IFoundItem[] foundPath)
        {
            Tracer.Assert(feature != null);
            Feature = feature;
            _foundPath = foundPath;
        }

        internal Result Result(Category category, RefAlignParam refAlignParam)
        {
            var featureResult = FeatureResult(category, refAlignParam);
            if (_foundPath.Length == 0)
                return featureResult;
            var converterResult = ObjectResult(category).LocalReferenceResult(refAlignParam);
            return featureResult.ReplaceArg(converterResult);
        }

        Result ObjectResult(Category category)
        {
            switch(_foundPath.Length)
            {
                case 0:

                    return null;
                case 1:
                    return _foundPath[0].Result(category);
            }

            NotImplementedMethod(category);
            return null;
        }

        Result FeatureResult(Category category, RefAlignParam refAlignParam)
        {
            var contextFeature = Feature as IContextFeature;
            if (contextFeature != null)
                return contextFeature.Result(category);
            return ((IFeature)Feature).Result(category, refAlignParam);
        }
    }
}