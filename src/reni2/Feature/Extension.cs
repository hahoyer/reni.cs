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
using HWClassLibrary.Helper;
using JetBrains.Annotations;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Syntax;
using Reni.Type;

namespace Reni.Feature
{
    static class Extension
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

            if(hasArg)
            {
                if (feature.MetaFunction != null)
                    return;

                Tracer.Assert(feature.Function != null, feature.Dump);
                Tracer.Assert(!feature.Function.IsImplicit, feature.Dump);
                return;
            }

            if (feature.Simple != null)
            {
                Tracer.Assert(feature.Function == null || !feature.Function.IsImplicit, ()=>"Ambiguity: Simple or ImplicitFunction? "+ feature.Dump());
                return;
            }

            Tracer.Assert(feature.Function != null, feature.Dump);
            Tracer.Assert(feature.Function.IsImplicit, feature.Dump);
        }

        internal static Simple Feature(Func<Category, Result> function) { return new Simple(function); }
        internal static Simple<T> Feature<T>(Func<Category, T, Result> function) { return new Simple<T>(function); }
        internal static Simple<T1, T2> Feature<T1, T2>(Func<Category, T1, T2, Result> function) { return new Simple<T1, T2>(function); }
        
        internal static Function Feature(Func<Category, IContextReference, TypeBase, Result> function)
        {
            return
                new Function(function);
        }
        
        internal static Function<T> Feature<T>(Func<Category, IContextReference, TypeBase, Result> function)
        {
            return
                new Function<T>(function);
        }
        
        internal static MetaFunction Feature(Func<ContextBase, Category, CompileSyntax, CompileSyntax, Result> function)
        {
            return
                new MetaFunction(function);
        }

        [UsedImplicitly]
        static bool _isPrettySearchPathHumanFriendly = true;

        internal static string PrettySearchPath(this System.Type type)
        {
            if(!_isPrettySearchPathHumanFriendly 
                || !type.IsGenericType 
                || type.GetGenericTypeDefinition() != typeof(ISearchPath<,>))
                return type.PrettyName();

            var types = type.GetGenericArguments();
            return PrettySearchPath(types[0]) + " -> " + types[1].PrettyName();
        }
    }
}