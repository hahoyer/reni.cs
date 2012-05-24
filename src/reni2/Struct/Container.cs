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
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
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

        protected override TokenData GetFirstToken() { return _firstToken; }

        protected override TokenData GetLastToken() { return _lastToken; }

        Container(TokenData leftToken, TokenData rightToken, CompileSyntax[] statements, DictionaryEx<string, int> dictionary, int[] converters)
            : base(leftToken, _nextObjectId++)
        {
            _firstToken = leftToken;
            _lastToken = rightToken;
            _statements = statements;
            _dictionary = dictionary;
            _converters = converters;
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

            public void Add(IParsedSyntax parsedSyntax)
            {
                while(parsedSyntax is DeclarationSyntax)
                {
                    var d = (DeclarationSyntax) parsedSyntax;
                    _dictionary.Add(d.Defineable.Name, _list.Count);
                    parsedSyntax = d.Definition;
                }

                if(parsedSyntax is ConverterSyntax)
                {
                    var body = ((ConverterSyntax) parsedSyntax).Body;
                    parsedSyntax = body;
                    _converters.Add(_list.Count);
                }

                _list.Add(((ReniParser.ParsedSyntax) parsedSyntax).ToCompiledSyntax());
            }

            public Container ToContainer(TokenData leftToken, TokenData rightToken) { return new Container(leftToken, rightToken, _list.ToArray(), _dictionary, _converters.ToArray()); }
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
            var dumpFile = ("struct." + ObjectId).FileHandle();
            var oldResult = dumpFile.String;
            var newResult = (_runId + DumpDataToString()).Replace("\n", "\r\n");
            if(oldResult == null || !oldResult.StartsWith(_runId))
            {
                oldResult = newResult;
                dumpFile.String = oldResult;
            }
            else
                Tracer.Assert(oldResult == newResult);
            return Tracer.FilePosn(dumpFile.FullName, 1, 0, FilePositionTag.Debug) + "see there" + "\n";
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

        internal ISearchPath<ISuffixFeature, StructureType> SearchFromRefToStruct(ISearchTarget target) { return FindStructFeature(target.StructFeatureName); }

        internal IStructFeature SearchFromStructContext(ISearchTarget defineable) { return FindStructFeature(defineable.StructFeatureName); }

        internal override Result ObtainResult(ContextBase context, Category category)
        {
            var innerResult = StructureResult(category - Category.Type, context, 0, EndPosition);
            return context.UniqueContainerContext(this).Result(category, innerResult);
        }

        Result InnerResult(Category category, ContextBase parent, int position)
        {
            Tracer.Assert(!Statements[position].IsLambda);
            return Result(category, parent, position, position);
        }

        Result Result(Category category, ContextBase parent, int accessPosition, int position)
        {
            var trace = ObjectId == -1 && accessPosition == 1 && position == 1;
            StartMethodDump(trace, category, parent, accessPosition, position);
            try
            {
                var uniqueChildContext = parent
                    .UniqueStructurePositionContext(this, accessPosition);
                Dump("Statements[position]", Statements[position]);
                BreakExecution();
                var result = Statements[position]
                    .Result(uniqueChildContext, category.Typed);
                Dump("result", result);
                return ReturnMethodDump(result.AutomaticDereference(), true);
            }
            finally
            {
                EndMethodDump();
            }
        }

        bool? InternalInnerIsDataLess(bool isQuick, ContextBase parent, int position)
        {
            var uniqueChildContext = parent
                .UniqueStructurePositionContext(this, position);
            return Statements[position].IsDataLessStructureElement(isQuick, uniqueChildContext);
        }

        internal bool? IsDataLess(bool isQuick, ContextBase parent, int accessPosition)
        {
            var trace = ObjectId == -1 && accessPosition == 2 && parent.ObjectId == 1;
            StartMethodDump(trace, isQuick, parent, accessPosition);
            try
            {
                var subStatementIds = accessPosition.Array(i => i).ToArray();
                Dump("subStatementIds", subStatementIds);
                BreakExecution();
                if(subStatementIds.Any(position => InternalInnerIsDataLess(true, parent, position) == false))
                    return ReturnMethodDump(false, true);
                var quickNonDataLess = subStatementIds
                    .Where(position => InternalInnerIsDataLess(true, parent, position) == null)
                    .ToArray();
                Dump("quickNonDataLess", quickNonDataLess);
                BreakExecution();
                if(quickNonDataLess.Length == 0)
                    return ReturnMethodDump(true, true);
                if(isQuick)
                    return ReturnMethodDump<bool?>(null, true);
                if(quickNonDataLess.Any(position => InternalInnerIsDataLess(false, parent, position) == false))
                    return ReturnMethodDump(false, true);
                if(quickNonDataLess.Any(position => InternalInnerIsDataLess(false, parent, position) == null))
                    return ReturnMethodDump<bool?>(null, true);
                return ReturnMethodDump(true, true);
            }
            finally
            {
                EndMethodDump();
            }
        }

        internal Result StructureResult(Category category, ContextBase parent, int fromPosition, int fromNotPosition)
        {
            var trace = ObjectId == -1 && category.HasSize;
            StartMethodDump(trace, category, parent, fromPosition, fromNotPosition);
            try
            {
                Dump("Statements", Statements);

                var results = (fromNotPosition - fromPosition)
                    .Array(i => fromPosition + i)
                    .Where(position => !Statements[position].IsLambda)
                    .Select(position => InnerResult(category, parent, position))
                    .Select(r => r.Align(parent.RefAlignParam.AlignBits))
                    .Select(r => r.LocalBlock(category))
                    .ToArray();
                Dump("results", results);
                BreakExecution();
                var result = results
                    .Aggregate(TypeBase.VoidResult(category), (current, next) => current + next);
                return ReturnMethodDump(result, true);
            }
            finally
            {
                EndMethodDump();
            }
        }

        internal Size StructureSize(ContextBase parent, int fromPosition, int fromNotPosition) { return StructureResult(Category.Size, parent, fromPosition, fromNotPosition).Size; }

        internal TypeBase AccessType(ContextBase parent, int accessPosition, int position)
        {
            var trace = ObjectId == -10 && accessPosition == 1 && position == 0;
            StartMethodDump(trace, parent, accessPosition, position);
            try
            {
                Dump("Statements[position]", Statements[position]);
                BreakExecution();
                var result = Result(Category.Type, parent, accessPosition, position).Type;
                return ReturnMethodDump(result, true);
            }
            finally
            {
                EndMethodDump();
            }
        }

        internal new bool IsLambda(int position) { return Statements[position].IsLambda; }
    }

    interface IStructFeature
        : ISearchPath<ISuffixFeature, StructureType>
    {
        IContextFeature ConvertToContextFeature(Structure accessPoint);
    }

    [Serializable]
    sealed class StructureFeature : ReniObject, IStructFeature
    {
        [EnableDump]
        readonly int _position;

        public StructureFeature(int position) { _position = position; }

        ISuffixFeature ISearchPath<ISuffixFeature, StructureType>.Convert(StructureType structureType) { return structureType.Structure.UniqueAccessFeature(_position); }
        IContextFeature IStructFeature.ConvertToContextFeature(Structure accessPoint) { return accessPoint.UniqueContextAccessFeature(_position); }
    }
}