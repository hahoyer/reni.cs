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

using System;
using System.Linq;
using System.Collections.Generic;
using HWClassLibrary.Debug;
using Reni.Basics;
using Reni.Sequence;
using Reni.Struct;
using Reni.Type;

namespace Reni
{
    abstract class SearchVisitor : ReniObject
    {
        internal abstract void Search(StructureType structureType);
        internal abstract void Search();

        internal SearchVisitor Child(SequenceType target) { return InternalChild(target); }
        internal SearchVisitor Child(AutomaticReferenceType target) { return InternalChild(target); }
        internal SearchVisitor Child(AccessType target) { return InternalChild(target); }
        internal SearchVisitor Child(TextItemType target) { return InternalChild(target); }

        internal void ChildSearch<TType>(TType target)
            where TType : IDumpShortProvider { InternalChild(target).Search(); }

        protected abstract SearchVisitor InternalChild<TType>(TType target)
            where TType : IDumpShortProvider;

        internal abstract bool IsSuccessFull { get; }
        internal abstract IConversionFunction[] ConversionFunctions { set; get; }
        internal void Add(IConversionFunction conversionFunction) { ConversionFunctions = ConversionFunctions.Concat(new[] {conversionFunction}).ToArray(); }

        internal void SearchAndConvert(TypeBase searchType, IContainerType argType)
        {
            if(IsSuccessFull)
                return;
            searchType.Search(this);
            if(!IsSuccessFull)
                return;

            Add(new ConversionFunction(argType));
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
            if(!IsSuccessFull)
                typeBase.Search(this);
        }

        internal override void Search(StructureType structureType) { structureType.SearchFeature(this); }
        internal override void Search()
        {
            if(!IsSuccessFull)
                InternalResult = Target as TFeature;
        }

        protected override SearchVisitor InternalChild<TType>(TType target) { return new ChildSearchVisitor<TFeature, TType>(this, target); }
    }

    interface ISearchTarget
    {
        string StructFeatureName { get; }
    }
}