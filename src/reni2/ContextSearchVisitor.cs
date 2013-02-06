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
using System.Linq;
using System.Collections.Generic;
using HWClassLibrary.Debug;
using Reni.Basics;
using Reni.Context;
using Reni.Feature;
using Reni.ReniParser;
using Reni.Type;

namespace Reni
{
    sealed class ContextSearchVisitor : RootSearchVisitor<IContextFeature>
    {
        internal ContextSearchVisitor(ISearchTarget target, ExpressionSyntax syntax)
            : base(target, syntax) { }

        internal ISearchResult SearchResult
        {
            get
            {
                if(IsSuccessFull)
                    return new ContextSearchResult(Result, ConversionFunctions);
                return null;
            }
        }

        internal void Search(ContextBase contextBase) { contextBase.Search(this); }

        internal void Search(Struct.Context context)
        {
            var accessPoint = context.Structure;
            var feature = accessPoint.Search(Target);
            if(feature == null)
                return;
            InternalResult = feature.ConvertToContextFeature(accessPoint);
            Add(new ConversionFunction(context));
        }

        sealed class ConversionFunction : ReniObject, IConversionFunction
        {
            readonly Struct.Context _parent;
            public ConversionFunction(Struct.Context parent) { _parent = parent; }
            Result IConversionFunction.Result(Category category) { return _parent.ObjectResult(category); }
        }
    }

    sealed class ContextSearchResult : SearchResult
    {
        internal ContextSearchResult(IContextFeature feature, IConversionFunction[] conversionFunctions)
            : base(feature, conversionFunctions) { }

        [DisableDump]
        protected override TypeBase DefiningType { get { return null; } }
    }
}