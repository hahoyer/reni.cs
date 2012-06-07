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

namespace Reni.Feature
{
    static class FeatureExtension
    {
        internal static TFeature CheckedConvert<TFeature, TType>(this ISearchPath<TFeature, TType> feature, TType target)
            where TFeature : class, IFeature
            where TType : IDumpShortProvider
        {
            if(feature == null)
                return null;
            return feature.Convert(target);
        }

        internal static string Dump(this IFeature feature) { return Tracer.Dump(feature); }

        internal static void AssertValid(this IFeature feature, bool hasArg)
        {
            Tracer.Assert(feature != null);

            if(feature.MetaFunction != null)
                return;

            if(hasArg)
                if(feature.Function != null)
                {
                    if(!feature.Function.IsImplicit)
                        return;
                    Tracer.AssertionFailed("!feature.Function.IsImplicit", feature.Dump);
                }

            if(feature.Function != null)
            {
                if(feature.Function.IsImplicit)
                    return;
                Tracer.AssertionFailed("feature.Function.IsImplicit", feature.Dump);
            }

            if(feature.Simple != null)
                return;

            Tracer.AssertionFailed("feature.Simple != null", feature.Dump);
        }
    }
}