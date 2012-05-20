#region Copyright (C) 2012

// 
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
using Reni.Feature;
using Reni.Type;


namespace Reni.TokenClasses
{
    sealed class ConcatArrays :
        Defineable
        , ISearchPath<IPrefixFeature, TypeBase>
        , ISearchPath<ISuffixFeature, Type.Array>
        , ISearchPath<ISearchPath<IPrefixFeature, AutomaticReferenceType>, TypeBase>
        , ISearchPath<ISearchPath<ISuffixFeature, AutomaticReferenceType>, Type.Array>
    {
        ISuffixFeature ISearchPath<ISuffixFeature, Type.Array>.Convert(Type.Array type) { return new Feature.RefAlignedFeature(type.ConcatArrays); }
        IPrefixFeature ISearchPath<IPrefixFeature, TypeBase>.Convert(TypeBase type) { return new RefAlignedPrefixFeature(type.CreateArray); }
        ISearchPath<IPrefixFeature, AutomaticReferenceType> ISearchPath<ISearchPath<IPrefixFeature, AutomaticReferenceType>, TypeBase>.Convert(TypeBase type) { return type.CreateArrayFromReferenceFeature; }
        ISearchPath<ISuffixFeature, AutomaticReferenceType> ISearchPath<ISearchPath<ISuffixFeature, AutomaticReferenceType>, Type.Array>.Convert(Type.Array type) { return type.ConcatArraysFromReferenceFeature; }
    }
}