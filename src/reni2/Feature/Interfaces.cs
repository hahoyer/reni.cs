#region Copyright (C) 2013

//     Project Reni2
//     Copyright (C) 2011 - 2013 Harald Hoyer
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

using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.ReniParser;
using Reni.Syntax;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Feature
{
    interface IFeatureImplementation
    {
        IMetaFunctionFeature MetaFunction { get; }
        IFunctionFeature Function { get; }
        ISimpleFeature Simple { get; }
    }

    interface ISimpleFeature
    {
        Result Result(Category category);
    }

    interface IFunctionFeature
    {
        /// <summary>
        ///     Result code contains CodeBase.Arg for argsType and ObjectReference for function object, if appropriate
        /// </summary>
        /// <param name="category"> </param>
        /// <param name="argsType"> </param>
        /// <returns> </returns>
        Result ApplyResult(Category category, TypeBase argsType);

        /// <summary>
        ///     Gets a value indicating whether this function requires implicit call (i. e. call without argument list).
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is implicit; otherwise, <c>false</c>.
        /// </value>
        [DisableDump]
        bool IsImplicit { get; }

        [DisableDump]
        IContextReference ObjectReference { get; }
    }

    interface IMetaFunctionFeature
    {
        Result ApplyResult(ContextBase contextBase, Category category, CompileSyntax left, CompileSyntax right);
    }

    interface ISearchResult
    {
        Result FunctionResult(ContextBase context, Category category, ExpressionSyntax syntax);
        Result SimpleResult(Category category);
        ISearchResult WithConversion(IProxyType proxyType);
    }

    interface IConversionFunction
    {
        Result Result(Category category);
    }

    interface ISearchObject
    {
        ISearchResult GetFeatureGenericized(ISearchTarget target);
    }

    interface ISearchTarget
    {
        ISearchResult GetFeature<TDefinable , TPath>()
            where TDefinable : Defineable;
    }

    interface ISymbolProvider<TDefinable, out TPath>
        where TDefinable : Defineable
    {
        TPath Feature { get; }
    }

    interface IPath<out TPath, in TProvider>
        where TProvider : TypeBase
    {
        TPath Convert(TProvider provider);
    }

}