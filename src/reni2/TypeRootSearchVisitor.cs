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

using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using Reni.Feature;
using Reni.ReniParser;
using Reni.Type;

namespace Reni
{
    sealed class TypeRootSearchVisitor<TFeature> : RootSearchVisitor<TFeature>
        where TFeature : class, IFeature
    {
        [EnableDump]
        readonly TypeBase _type;

        internal TypeRootSearchVisitor(ISearchTarget target, TypeBase type, ExpressionSyntax syntax)
            : base(target, syntax) { _type = type; }

        [EnableDump]
        internal IEnumerable<string> ProbeStr { get { return Probes.Values.Select(probe => probe.LogDump); } }

        internal SearchResult SearchResult
        {
            get
            {
                if(IsSuccessFull)
                    return new TypeSearchResult(ResultProvider, ConversionFunctions, _type);
                return null;
            }
        }
    }
}