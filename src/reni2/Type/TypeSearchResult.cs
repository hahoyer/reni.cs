#region Copyright (C) 2013

//     Project Reni2
//     Copyright (C) 2013 - 2013 Harald Hoyer
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
using Reni.Context;
using Reni.Feature;
using Reni.ReniParser;

namespace Reni.Type
{
    sealed class TypeSearchResult<TPath> : DumpableObject, ISearchResult
    {
        [EnableDump]
        readonly TypeBase _type;
        [EnableDump]
        readonly TPath _feature;

        internal TypeSearchResult(TypeBase type, TPath feature)
        {
            _type = type;
            _feature = feature;
        }

        Result ISearchResult.FunctionResult(ContextBase context, Category category, ExpressionSyntax syntax)
        {
            return CallDescriptor
                .Result(category, context, syntax.Left, syntax.Right);
        }
        Result ISearchResult.SimpleResult(Category category)
        {
            NotImplementedMethod(category);
            return null;
        }
        ISearchResult ISearchResult.WithConversion(IConverter converter) { return new SearchResultWithConversion(this, converter); }

        CallDescriptor CallDescriptor { get { return new CallDescriptor(_type, Feature, category => null); } }

        IFeatureImplementation Feature
        {
            get
            {
                var result = _feature as IFeatureImplementation;
                if(result != null)
                    return result;
                NotImplementedMethod();
                return null;
            }
        }
    }

    sealed class SearchResultWithConversion : DumpableObject, ISearchResult
    {
        [EnableDump]
        readonly ISearchResult _searchResult;
        [EnableDump]
        readonly IConverter _converter;

        internal SearchResultWithConversion(ISearchResult searchResult, IConverter converter)
        {
            _searchResult = searchResult;
            _converter = converter;
        }
        Result ISearchResult.FunctionResult(ContextBase context, Category category, ExpressionSyntax syntax)
        {
            NotImplementedMethod(context, category, syntax);
            return null;
        }
        Result ISearchResult.SimpleResult(Category category)
        {
            NotImplementedMethod(category);
            return null;
        }
        ISearchResult ISearchResult.WithConversion(IConverter converter)
        {
            NotImplementedMethod(converter);
            return null;
        }
    }
}