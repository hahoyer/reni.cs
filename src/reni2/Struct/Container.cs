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
        internal override bool? IsDataLess
        {
            get
            {
                return Statements
                    .All(syntax => syntax.IsDataLess == true);
            }
        }

        [DisableDump]
        internal Size IndexSize { get { return Size.AutoSize(Statements.Length); } }

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

                _list.Add(((ParsedSyntax) parsedSyntax).ToCompiledSyntax());
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

        internal static Container Create(TokenData leftToken, TokenData rightToken, ParsedSyntax parsedSyntax)
        {
            var result = new PreContainer();
            result.Add(parsedSyntax);
            return result.ToContainer(leftToken, rightToken);
        }

        internal static Container Create(ParsedSyntax parsedSyntax) { return Create(parsedSyntax.FirstToken, parsedSyntax.LastToken, parsedSyntax); }

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

        internal StructurePosition Find(string name)
        {
            if(name == null)
                return null;
            if(!Dictionary.ContainsKey(name))
                return null;

            var position = Dictionary[name];
            return new StructurePosition(position);
        }

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
            var trace = new[] {-1}.Contains(ObjectId) && accessPosition == 1 && position == 1;
            StartMethodDump(trace, category, parent, accessPosition, position);
            try
            {
                var uniqueChildContext = parent
                    .UniqueStructurePositionContext(this, accessPosition);
                Dump("Statements[position]", Statements[position]);
                BreakExecution();
                var result = Statements[position]
                    .Result(uniqueChildContext, category.Typed)
                    .SmartUn<FunctionType>();
                Dump("result", result);
                return ReturnMethodDump(result.AutomaticDereferenceResult());
            }
            finally
            {
                EndMethodDump();
            }
        }

        bool InternalInnerIsDataLess(ContextBase parent, int position)
        {
            var uniqueChildContext = parent
                .UniqueStructurePositionContext(this, position);
            return Statements[position].IsDataLessStructureElement(uniqueChildContext);
        }

        bool? InternalInnerIsDataLess(int position) { return Statements[position].IsDataLess; }

        internal bool ObtainIsDataLess(ContextBase parent, int accessPosition)
        {
            var trace = ObjectId == -10 && accessPosition == 3 && parent.ObjectId == 4;
            StartMethodDump(trace, parent, accessPosition);
            try
            {
                var subStatementIds = accessPosition.Array(i => i).ToArray();
                Dump("subStatementIds", subStatementIds);
                BreakExecution();
                if(subStatementIds.Any(position => InternalInnerIsDataLess(position) == false))
                    return ReturnMethodDump(false);
                var quickNonDataLess = subStatementIds
                    .Where(position => InternalInnerIsDataLess(position) == null)
                    .ToArray();
                Dump("quickNonDataLess", quickNonDataLess);
                BreakExecution();
                if(quickNonDataLess.Length == 0)
                    return ReturnMethodDump(true);
                if(quickNonDataLess.Any(position => InternalInnerIsDataLess(parent, position) == false))
                    return ReturnMethodDump(false);
                return ReturnMethodDump(true);
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
                    .Aggregate(parent.RootContext.VoidResult(category), (current, next) => current + next);
                return ReturnMethodDump(result);
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
                return ReturnMethodDump(result);
            }
            finally
            {
                EndMethodDump();
            }
        }

        internal string[] DataIndexList(ContextBase parent)
        {
            return Statements
                .Length
                .Array(i => i)
                .Where(i => !InternalInnerIsDataLess(parent, i))
                .Select(i=> i.ToString() + "="+ InnerResult(Category.Size,parent,i).Size.ToString())
                .ToArray();
        }
    }

    [Serializable]
    sealed class StructurePosition : ReniObject, ISearchPath<ISuffixFeature, StructureType>
    {
        [EnableDump]
        readonly int _position;

        internal StructurePosition(int position) { _position = position; }

        ISuffixFeature ISearchPath<ISuffixFeature, StructureType>.Convert(StructureType structureType) { return Convert(structureType.Structure); }
        internal IContextFeature ConvertToContextFeature(Structure accessPoint) { return Convert(accessPoint); }
        AccessFeature Convert(Structure accessPoint) { return accessPoint.UniqueAccessFeature(_position); }
    }
}