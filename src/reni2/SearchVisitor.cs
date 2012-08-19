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
using HWClassLibrary.Helper;
using Reni.Basics;
using Reni.Context;
using Reni.Feature;
using Reni.Struct;
using Reni.Type;
using Reni.Validation;

namespace Reni
{
    abstract class SearchVisitor : ReniObject
    {
        internal static bool Trace;

        protected abstract SearchVisitor PathItem<TType>(TType target)
            where TType : IDumpShortProvider;

        internal abstract bool IsSuccessFull { get; }
        internal abstract IConversionFunction[] ConversionFunctions { set; get; }
        internal void Add(IConversionFunction conversionFunction) { ConversionFunctions = ConversionFunctions.Concat(new[] {conversionFunction}).ToArray(); }

        protected abstract void SearchNameSpace(StructureType structureType);
        internal abstract void Search(string item);

        internal void Search<TType>(TType target, Func<TypeBase> getChild)
            where TType : IDumpShortProvider
        {
            if(Trace)
                Tracer.FlaggedLine(1, " >>> " + target.DumpShort());
            if(Trace)
                Tracer.IndentStart();
            try
            {
                var pathItemVisitor = PathItem(target);

                if(Trace)
                    Tracer.FlaggedLine("pathItemVisitor.Search()");
                pathItemVisitor.Search("item");
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
                    Tracer.FlaggedLine(1, " <<< " + target.DumpShort());
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
        internal readonly DictionaryEx<System.Type, string> Probe;

        protected SearchVisitor(DictionaryEx<System.Type, string> probe) { Probe = probe; }

        internal abstract TFeature InternalResult { set; }
        internal abstract ISearchTarget Target { get; }

        protected override void SearchNameSpace(StructureType structureType) { structureType.SearchNameSpace(this); }
        internal override void Search(string item)
        {
            if(Trace)
                Tracer.Line(typeof(TFeature).PrettySearchPath());
            Tracer.Assert(!IsSuccessFull, () => Tracer.Dump(Probe));
            if(Probe.Keys.Contains(typeof(TFeature)))
            {
                Tracer.Assert(Probe[typeof(TFeature)] == item);
                return;
            }
            AddProbe(typeof(TFeature), item);
            InternalResult = Target as TFeature;
        }
        internal override void Search(IssueType target)
        {
            var searchResult = target.SearchResult(Target);
            var internalResult = searchResult as TFeature;
            Tracer.Assert(internalResult != null, ()=>typeof(TFeature).PrettyName());
            InternalResult = internalResult;
        }
        void AddProbe(System.Type testType, string item)
        {
            Tracer.Assert(!Probe.Keys.Contains(testType), "Target=" + Target + "\nProbe=" + Tracer.Dump(Probe) + "\ntestType=" + testType.PrettyName());
            Probe.Add(testType, item);
        }

        protected override SearchVisitor PathItem<TType>(TType target) { return new PathItemSearchVisitor<TFeature, TType>(this, target); }
    }

    interface ISearchTarget 
    {
        string StructFeatureName { get; }
    }
}