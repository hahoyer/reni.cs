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
using System.Linq;
using System.Collections.Generic;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Basics;
using Reni.Feature;
using Reni.ReniParser;
using Reni.Struct;
using Reni.Type;
using Reni.Validation;

namespace Reni
{
    abstract class SearchVisitor : ReniObject
    {
        internal static bool Trace;

        protected abstract SearchVisitor PathItem<TType>(TType target);

        internal abstract bool IsSuccessFull { get; }
        internal abstract IConversionFunction[] ConversionFunctions { set; get; }
        internal void Add(IConversionFunction conversionFunction) { ConversionFunctions = ConversionFunctions.Concat(new[] {conversionFunction}).ToArray(); }

        protected abstract void SearchNameSpace(StructureType structureType);
        internal abstract void Search();

        internal void Search<TType>(TType target, Func<TypeBase> getChild)
        {
            if(Trace)
                Tracer.FlaggedLine(1, " >>> " + target.NodeDump());
            if(Trace)
                Tracer.IndentStart();
            try
            {
                var pathItemVisitor = PathItem(target);

                if(Trace)
                    Tracer.FlaggedLine("pathItemVisitor.Search()");
                pathItemVisitor.Search();
                if(IsSuccessFull)
                    return;

                if(getChild == null)
                    return;

                var child = getChild();
                if(child == null)
                    return;

                if(Trace)
                    Tracer.FlaggedLine("child.Search(pathItemVisitor)");
                child.Search(pathItemVisitor);
                if(IsSuccessFull)
                    return;

                var isc = target as IProxyType;
                if(isc == null)
                    return;

                Tracer.Assert(isc.Converter.TargetType == child);
                if(Trace)
                    Tracer.FlaggedLine("child.Search(this)");
                child.Search(this);

                if(!IsSuccessFull)
                    return;
                Add(new ConversionFunction(isc));
            }
            finally
            {
                if(Trace)
                    Tracer.IndentEnd();
                if(Trace)
                    Tracer.FlaggedLine(1, " <<< " + target.NodeDump());
            }
        }

        internal void Search(StructureType structureType)
        {
            SearchNameSpace(structureType);
            if(IsSuccessFull)
                return;
            Search(structureType, null);
        }

        internal abstract void Search(IssueType target);
    }

    interface IConversionFunction
    {
        Result Result(Category category);
    }

    abstract class SearchVisitor<TFeature> : SearchVisitor
        where TFeature : class, ISearchPath
    {
        readonly ExpressionSyntax _syntax;

        protected SearchVisitor(ExpressionSyntax syntax) { _syntax = syntax; }

        internal abstract TFeature InternalResult { set; }
        internal abstract ISearchTarget Target { get; }
        internal abstract DictionaryEx<System.Type, Probe> Probes { get; }

        protected override void SearchNameSpace(StructureType structureType) { structureType.SearchNameSpace(this); }
        internal override void Search()
        {
            Tracer.Assert(!IsSuccessFull, () => Tracer.Dump(Probes));
            Probes.Ensure(typeof(TFeature));
            InternalResult = Target as TFeature;
        }

        internal override void Search(IssueType target)
        {
            var searchResult = target.SearchResult(Target, _syntax);
            var internalResult = searchResult as TFeature;
            //Tracer.Assert(internalResult != null, ()=>typeof(TFeature).PrettyName());
            InternalResult = internalResult;
        }

        protected override SearchVisitor PathItem<TType>(TType target) { return new PathItemSearchVisitor<TFeature, TType>(this, target, _syntax); }
    }

    interface ISearchTarget
    {
        string StructFeatureName { get; }
        IEnumerable<TPath> GetFeature<TPath>(TypeBase typeBase) where TPath : class;
    }
}