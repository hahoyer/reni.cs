//     Compiler for programming language "Reni"
//     Copyright (C) 2011 Harald Hoyer
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
using Reni.Sequence;
using Reni.Struct;
using Reni.Syntax;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni
{
    abstract class SearchVisitor : ReniObject
    {
        internal void ChildSearch<TType>(TType target)
            where TType : IDumpShortProvider, IResultProvider { InternalChild(target).Search(); }

        internal SearchVisitor Child(SequenceType target) { return InternalChild(target); }
        internal SearchVisitor Child(AutomaticReferenceType target) { return InternalChild(target); }
        internal SearchVisitor Child(AccessType target) { return InternalChild(target); }
        internal SearchVisitor Child(TextItemType target) { return InternalChild(target); }

        protected abstract void Search(StructureType structureType);
        internal abstract ConversionFunction[] ConversionFunctions { set; get; }

        internal abstract void Search();

        protected abstract SearchVisitor InternalChild<TType>(TType target)
            where TType : IResultProvider, IDumpShortProvider;

        internal abstract bool IsSuccessFull { get; }
        internal void Add(ConversionFunction conversionFunction) { ConversionFunctions = ConversionFunctions.Concat(new[] {conversionFunction}).ToArray(); }
    }

    abstract class SearchVisitor<TFeature> : SearchVisitor
        where TFeature : class
    {
        internal abstract TFeature InternalResult { set; }
        internal abstract Defineable Defineable { get; }


        internal void Search(TypeBase typeBase)
        {
            if(!IsSuccessFull)
                typeBase.Search(this);
        }

        protected override void Search(StructureType structureType) { structureType.Search(this); }
        internal override void Search()
        {
            if(!IsSuccessFull)
                InternalResult = Defineable.Check<TFeature>();
        }

        protected override SearchVisitor InternalChild<TType>(TType target) { return new ChildSearchVisitor<TFeature, TType>(this, target); }
    }
}