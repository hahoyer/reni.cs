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
        static readonly DictionaryEx<Func<ContextBase, Category, CompileSyntax, CompileSyntax, Result>, MetaFunction> _metaFunctionCache
            = new DictionaryEx<Func<ContextBase, Category, CompileSyntax, CompileSyntax, Result>, MetaFunction>
                (function => new MetaFunction(function));

        static readonly DictionaryEx<Func<Category, Result>, Simple> _simpleCache
            = new DictionaryEx<Func<Category, Result>, Simple>(function => new Simple(function));

        internal static TFeature CheckedConvert<TFeature, TType>(this ISearchPath<TFeature, TType> feature, TType target)
            where TFeature : class, IFeature
        {
            if(feature == null)
                return null;
            return feature.Convert(target);
        }

        internal static string Dump(this IFeature feature) { return Tracer.Dump(feature); }

        internal static Simple Feature(Func<Category, Result> function) { return _simpleCache[function]; }
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
            return _metaFunctionCache[function];
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

        internal static ISimpleFeature SimpleFeature(this IFeature feature, bool hasNoArg)
        {
            if(!hasNoArg)
                return null;

            var function = feature.Function;
            if(function != null && function.IsImplicit)
                return null;

            return feature.Simple;
        }

        internal static bool HasCodeArgs(this IFeature feature, ContextBase context, TypeBase objectType, Func<Category, Result> converterResult, CompileSyntax right)
        {
            return context
                .Result(Category.CodeArgs, feature, objectType, right)
                .ReplaceArg(converterResult)
                .CodeArgs
                .HasArg;
        }

        internal static bool IsEqual(this ISearchPath one, ISearchPath other)
        {
            if(one == other)
                return true;
            if(one.GetType() != other.GetType())
                return false;
            var simple = one as SimpleBase;
            if(simple != null)
                return simple.IsEqual((SimpleBase)other);
            Tracer.ConditionalBreak(true);
            return false;
        }
    }
}