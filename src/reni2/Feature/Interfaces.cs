#region Copyright (C) 2012

//     Project Reni2
//     Copyright (C) 2011 - 2012 Harald Hoyer
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
using Reni.Syntax;
using Reni.Type;

namespace Reni.Feature
{
    interface ISearchPath
    {}

    interface ISearchPath<out TOutType, in TInType> : ISearchPath
        where TOutType : ISearchPath
    {
        TOutType Convert(TInType type);
    }

    interface IFeature : ISearchPath
    {
        IMetaFunctionFeature MetaFunction { get; }
        IFunctionFeature Function { get; }
        ISimpleFeature Simple { get; }
    }

    interface ISuffixFeature : IFeature
    { }

    interface IPrefixFeature : IFeature
    { }

    interface IContextFeature : IFeature
    { }

    interface ISimpleFeature
    {
        Result Result(Category category);
    }

    interface IFunctionFeature
    {
        /// <summary>
        /// Result code contains CodeBase.Arg for argsType and ObjectReference for function object, 
        /// if appropriate
        /// </summary>
        /// <param name="category"></param>
        /// <param name="argsType"></param>
        /// <returns></returns>
        Result ApplyResult(Category category, TypeBase argsType);
        [DisableDump]
        bool IsImplicit { get; }
        [DisableDump]
        IContextReference ObjectReference { get; }
    }

    interface IFunctionalFeatureSpecial
    {
        Result ApplyResult(Category category, Result argsResult, RefAlignParam refAlignParam);
    }

    interface IMetaFunctionFeature
    {
        Result ApplyResult(ContextBase contextBase, Category category, CompileSyntax left, CompileSyntax right);
    }
}