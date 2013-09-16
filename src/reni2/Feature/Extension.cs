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
using hw.Helper;
using JetBrains.Annotations;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Syntax;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Feature
{
    static class Extension
    {
        static readonly FunctionCache<Func<ContextBase, Category, CompileSyntax, CompileSyntax, Result>, MetaFunction> _metaFunctionCache
            = new FunctionCache<Func<ContextBase, Category, CompileSyntax, CompileSyntax, Result>, MetaFunction>
                (function => new MetaFunction(function));

        static readonly FunctionCache<Func<Category, Result>, Simple> _simpleCache
            = new FunctionCache<Func<Category, Result>, Simple>(function => new Simple(function));

        internal static Simple Feature(Func<Category, Result> function) { return _simpleCache[function]; }

        internal static Simple<T1> Feature<T1>(Func<Category, T1, Result> function)
            where T1 : TypeBase
        {
            return
                new Simple<T1>(function);
        }

        internal static Simple<T1, T2> Feature<T1, T2>(Func<Category, T1, T2, Result> function)
            where T1 : TypeBase
            where T2 : TypeBase
        {
            return
                new Simple<T1, T2>(function);
        }

        internal static string Dump(this IFeatureImplementation feature) { return Tracer.Dump(feature); }
        internal static Function Feature(Func<Category, IContextReference, TypeBase, Result> function)
        {
            return
                new Function(function);
        }

        internal static MetaFunction Feature(Func<ContextBase, Category, CompileSyntax, CompileSyntax, Result> function) { return _metaFunctionCache[function]; }

#pragma warning disable 0414
        [UsedImplicitly]
        static bool _isPrettySearchPathHumanFriendly = true;


        internal static ISimpleFeature SimpleFeature(this IFeatureImplementation feature, bool hasNoArg)
        {
            if(!hasNoArg)
                return null;

            var function = feature.Function;
            if(function != null && function.IsImplicit)
                return null;

            return feature.Simple;
        }

        internal static bool HasCodeArgs(this IFeatureImplementation feature, ContextBase context, TypeBase objectType, Func<Category, Result> converterResult, CompileSyntax right)
        {
            return context
                .Result(Category.CodeArgs, feature, objectType, right)
                .ReplaceArg(converterResult)
                .CodeArgs
                .HasArg;
        }

        internal static ISearchResult GetFeature<TDefinable>(this ISearchTarget target, TDefinable definable)
            where TDefinable : Defineable { return target.GetFeature<TDefinable, IFeatureImplementation>(); }
    }
}