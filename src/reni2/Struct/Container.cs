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
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using HWClassLibrary.IO;
using HWClassLibrary.TreeStructure;
using Reni.Basics;
using Reni.Context;
using Reni.Feature;
using Reni.Parser;
using Reni.ReniParser;
using Reni.Syntax;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Struct
{
    /// <summary>
    ///     Structured data, context free version
    /// </summary>
    [Serializable]
    sealed class Container : CompileSyntax, IDumpShortProvider
    {
        readonly TokenData _firstToken;
        readonly TokenData _lastToken;
        readonly CompileSyntax[] _statements;
        readonly DictionaryEx<string, int> _dictionary;
        readonly int[] _converters;
        readonly int[] _properties;
        static readonly string _runId = Compiler.FormattedNow + "\n";
        public static bool IsInContainerDump;
        static bool _isInsideFileDump;
        static int _nextObjectId;

        [Node]
        internal CompileSyntax[] Statements { get { return _statements; } }

        [DisableDump]
        internal int EndPosition { get { return Statements.Length; } }

        [Node]
        [SmartNode]
        internal DictionaryEx<string, int> Dictionary { get { return _dictionary; } }

        [Node]
        [SmartNode]
        internal int[] Converters { get { return _converters; } }

        [Node]
        [SmartNode]
        internal int[] Properties { get { return _properties; } }

        protected override TokenData GetFirstToken() { return _firstToken; }

        protected override TokenData GetLastToken() { return _lastToken; }

        Container(TokenData leftToken, TokenData rightToken, CompileSyntax[] statements, DictionaryEx<string, int> dictionary, int[] converters, int[] properties)
            : base(leftToken, _nextObjectId++)
        {
            _firstToken = leftToken;
            _lastToken = rightToken;
            _statements = statements;
            _dictionary = dictionary;
            _converters = converters;
            _properties = properties;
        }

        internal override CompileSyntax ToCompiledSyntax() { return this; }

        [DisableDump]
        internal int IndexSize { get { return BitsConst.AutoSize(Statements.Length); } }

        internal override string DumpShort() { return "container." + ObjectId; }

        sealed class PreContainer : ReniObject
        {
            readonly List<CompileSyntax> _list = new List<CompileSyntax>();
            readonly DictionaryEx<string, int> _dictionary = new DictionaryEx<string, int>();
            readonly List<int> _converters = new List<int>();
            readonly List<int> _properties = new List<int>();

            public void Add(IParsedSyntax parsedSyntax)
            {
                while(parsedSyntax is DeclarationSyntax)
                {
                    var d = (DeclarationSyntax) parsedSyntax;
                    _dictionary.Add(d.Defineable.Name, _list.Count);
                    parsedSyntax = d.Definition;
                    if(d.IsProperty)
                        _properties.Add(_list.Count);
                }

                if(parsedSyntax is ConverterSyntax)
                {
                    var body = ((ConverterSyntax) parsedSyntax).Body;
                    parsedSyntax = body;
                    _converters.Add(_list.Count);
                }

                _list.Add(((ReniParser.ParsedSyntax) parsedSyntax).ToCompiledSyntax());
            }

            public Container ToContainer(TokenData leftToken, TokenData rightToken) { return new Container(leftToken, rightToken, _list.ToArray(), _dictionary, _converters.ToArray(), _properties.ToArray()); }
        }


        internal static Container Create(TokenData leftToken, TokenData rightToken, List<IParsedSyntax> parsed)
        {
            var result = new PreContainer();
            foreach(var parsedSyntax in parsed)
                result.Add(parsedSyntax);
            return result.ToContainer(leftToken, rightToken);
        }

        internal static Container Create(TokenData leftToken, TokenData rightToken, ReniParser.ParsedSyntax parsedSyntax)
        {
            var result = new PreContainer();
            result.Add(parsedSyntax);
            return result.ToContainer(leftToken, rightToken);
        }

        internal static Container Create(ReniParser.ParsedSyntax parsedSyntax) { return Create(parsedSyntax.FirstToken, parsedSyntax.LastToken, parsedSyntax); }

        public override string DumpData()
        {
            var isInsideFileDump = _isInsideFileDump;
            _isInsideFileDump = true;
            var result = isInsideFileDump ? DumpDataToString() : DumpDataToFile();
            _isInsideFileDump = isInsideFileDump;
            return result;
        }

        string DumpDataToFile()
        {
            var dumpFile = File.m("struct." + ObjectId);
            var oldResult = dumpFile.String;
            var newResult = (_runId + DumpDataToString()).Replace("\n", "\r\n");
            if(oldResult == null || !oldResult.StartsWith(_runId))
            {
                oldResult = newResult;
                dumpFile.String = oldResult;
            }
            else
                Tracer.Assert(oldResult == newResult);
            return Tracer.FilePosn(dumpFile.FullName, 1, 0, "see there") + "\n";
        }

        string DumpDataToString()
        {
            var isInDump = IsInContainerDump;
            IsInContainerDump = true;
            var result = base.DumpData();
            IsInContainerDump = isInDump;
            return result;
        }

        string IDumpShortProvider.DumpShort() { return DumpShort(); }

        IStructFeature FindStructFeature(string name)
        {
            if(Dictionary.ContainsKey(name))
            {
                var position = Dictionary[name];
                return new StructureFeature(position);
            }
            return null;
        }

        internal ISearchPath<IFeature, StructureType> SearchFromRefToStruct(Defineable defineable) { return FindStructFeature(defineable.Name); }

        internal IStructFeature SearchFromStructContext(Defineable defineable) { return FindStructFeature(defineable.Name); }

        internal override Result ObtainResult(ContextBase context, Category category)
        {
            var innerResult = ConstructionResult(category - Category.Type, context, 0, EndPosition);
            return context.UniqueContainerContext(this).Result(category, innerResult);
        }

        Result InternalInnerResult(Category category, ContextBase parent, int accessPosition, int position)
        {
            var trace = ObjectId == -10 && accessPosition == 2 && position == 1;
            StartMethodDump(trace, category, parent, accessPosition, position);
            try
            {
                var uniqueChildContext = parent
                    .UniqueChildContext(this, accessPosition);
                Dump("Statements[position]", Statements[position]);
                BreakExecution();
                var result = Statements[position]
                    .Result(uniqueChildContext, category.Typed);
                return ReturnMethodDump(result.AutomaticDereference(), true);
            }
            finally
            {
                EndMethodDump();
            }
        }

        Result InnerResult(Category category, ContextBase parent, int accessPosition, int position)
        {
            Tracer.Assert(!(category.HasCode));
            return InternalInnerResult(category, parent, accessPosition, position);
        }

        internal Result ConstructionResult(Category category, ContextBase parent, int fromPosition, int fromNotPosition)
        {
            var result = TypeBase.VoidResult(category);
            for(var position = fromPosition; position < fromNotPosition; position++)
                result = result.Sequence(ConstructorResult(category, parent, position));
            return result;
        }

        internal override bool? FlatIsDataLess(ContextBase context) { return FlatIsDataLess(context, EndPosition); }

        internal bool? FlatIsDataLess(ContextBase parent, int accessPosition) 
        {
            var context = parent
                .UniqueChildContext(this, accessPosition);
            var subStatements = Statements.Take(accessPosition).ToArray();
            if(subStatements.Any(s => s.QuickIsDataLess(context) == false))
                return false;
            var listQuick = subStatements
                .Where(s => s.QuickIsDataLess(context) == null)
                .ToArray();

            if(listQuick.Any(s => s.FlatIsDataLess(context) == false))
                return false;
            var listFlat = listQuick
                .Where(s => s.FlatIsDataLess(context) == null)
                .ToArray();

            if(listFlat.Length == 0)
                return true;

            return null;
        }

        internal bool IsDataLess(ContextBase parent, int accessPosition)
        {
            var context = parent
                .UniqueChildContext(this, accessPosition);

            var result = FlatIsDataLess(context, accessPosition);
            if(result != null)
                return result.Value;

            var subStatements = Statements.Take(accessPosition).ToArray();
            if (subStatements.Any(s => !s.IsDataLess(context)))
                return false;
            return subStatements.All(s => s.IsDataLess(context));
        }

        Result ConstructorResult(Category category, ContextBase parent, int position)
        {
            StartMethodDump(ObjectId == -10 && position == 0 && category.HasIsDataLess, category, parent, position);
            try
            {
                var internalInnerResult = InternalInnerResult(category, parent, position + 1, position);
                Dump("internalInnerResult", internalInnerResult);
                var alignedResult = internalInnerResult.Align(parent.RefAlignParam.AlignBits);
                Dump("alignedResult", alignedResult);
                BreakExecution();
                var result = alignedResult.LocalBlock(category);
                return ReturnMethodDump(result, true);
            }
            finally
            {
                EndMethodDump();
            }
        }

        internal Size InnerSize(ContextBase parent, int position) { return InnerResult(Category.Size, parent, EndPosition, position).Size; }
        internal Size ConstructionSize(ContextBase parent, int fromPosition, int fromNotPosition) { return ConstructionResult(Category.Size, parent, fromPosition, fromNotPosition).Size; }
        internal TypeBase InnerType(ContextBase parent, int accessPosition, int position) { return InnerResult(Category.Type, parent, accessPosition, position).Type; }

        internal new bool IsLambda(int position) { return Statements[position].IsLambda; }
        internal bool IsProperty(int position) { return Properties.Contains(position); }
    }

    interface IStructFeature
        : ISearchPath<IFeature, StructureType>
    {
        IContextFeature ConvertToContextFeature(Structure accessPoint);
    }

    [Serializable]
    sealed class StructureFeature : ReniObject, IStructFeature
    {
        [EnableDump]
        readonly int _position;

        public StructureFeature(int position) { _position = position; }

        AccessFeature Convert(Structure contextObject) { return contextObject.UniqueAccessFeature(_position); }

        IFeature ISearchPath<IFeature, StructureType>.Convert(StructureType structureType) { return Convert(structureType.Structure); }
        IContextFeature IStructFeature.ConvertToContextFeature(Structure accessPoint) { return Convert(accessPoint); }
    }
}