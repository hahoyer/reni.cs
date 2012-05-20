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
using Reni.Struct;
using Reni.Type;

namespace Reni
{
    abstract class SearchVisitor : ReniObject
    {
        protected abstract SearchVisitor PathItem<TType>(TType target)
            where TType : IDumpShortProvider;

        internal abstract bool IsSuccessFull { get; }
        internal abstract IConversionFunction[] ConversionFunctions { set; get; }
        internal void Add(IConversionFunction conversionFunction) { ConversionFunctions = ConversionFunctions.Concat(new[] { conversionFunction }).ToArray(); }

        internal abstract void Search(StructureType structureType);
        internal abstract void Search();

        internal void SearchAtPath<TType>(TType target)
            where TType : IDumpShortProvider
        {
            if(IsSuccessFull)
                return;
            PathItem(target).Search();
        }

        internal void SearchWithPath<TType>(TypeBase childType, TType target)
            where TType : IDumpShortProvider
        {
            if (IsSuccessFull)
                return;
            childType.Search(PathItem(target));
        }

        internal void SearchAndConvert(TypeBase searchType, IContainerType containerType)
        {
            if(IsSuccessFull)
                return;
            searchType.Search(this);
            if(!IsSuccessFull)
                return;

            Add(new ConversionFunction(containerType));
        }
    }

    interface IConversionFunction
    {
        Result Result(Category category);
    }

    abstract class SearchVisitor<TFeature> : SearchVisitor
        where TFeature : class
    {
        internal abstract TFeature InternalResult { set; }
        internal abstract ISearchTarget Target { get; }


        internal void Search(TypeBase typeBase)
        {
            if(IsSuccessFull)
                return;
            typeBase.Search(this);
        }

        internal override void Search(StructureType structureType) { structureType.SearchFeature(this); }
        internal override void Search()
        {
            if(IsSuccessFull)
                return;
            InternalResult = Target as TFeature;
        }

        protected override SearchVisitor PathItem<TType>(TType target) { return new PathItemSearchVisitor<TFeature, TType>(this, target); }
    }

    interface ISearchTarget
    {
        string StructFeatureName { get; }
    }
}