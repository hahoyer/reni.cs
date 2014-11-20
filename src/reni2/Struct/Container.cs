using System;
using System.Collections.Generic;
using hw.Helper;
using System.Linq;
using hw.Debug;
using hw.Forms;
using hw.Parser;
using hw.Scanner;
using Reni.Basics;
using Reni.Context;
using Reni.ReniParser;
using Reni.ReniSyntax;
using Reni.Type;

namespace Reni.Struct
{
    /// <summary>
    ///     Structured data, context free version
    /// </summary>
    sealed class Container : CompileSyntax
    {
        readonly CompileSyntax[] _statements;
        readonly Dictionary<string, int> _dictionary;
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
        internal Dictionary<string, int> Dictionary { get { return _dictionary; } }

        [Node]
        [SmartNode]
        internal int[] Converters { get { return _converters; } }

        internal Container
            (
            SourcePart token,
            CompileSyntax[] statements,
            Dictionary<string, int> dictionary,
            int[] converters)
            : base(token, _nextObjectId++)
        {
            _statements = statements;
            _dictionary = dictionary;
            _converters = converters;
        }

        [DisableDump]
        internal override CompileSyntax ToCompiledSyntax { get { return this; } }
        [DisableDump]
        internal override bool? Hllw
        {
            get
            {
                return Statements
                    .All(syntax => syntax.Hllw == true);
            }
        }

        [DisableDump]
        internal Size IndexSize { get { return Size.AutoSize(Statements.Length); } }

        protected override string GetNodeDump() { return "container." + ObjectId; }

        [DisableDump]
        protected override ParsedSyntax[] Children { get { return Statements.ToArray<ParsedSyntax>(); } }

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
            var trace = ObjectId.In(-1) && accessPosition >= 0 && position >= 0 && category.HasCode;
            StartMethodDump(trace, category, parent, accessPosition, position);
            try
            {
                var uniqueChildContext = parent
                    .UniqueStructurePositionContext(this, accessPosition);
                Dump("Statements[position]", Statements[position]);
                BreakExecution();
                var result1 = Statements[position]
                    .Result(uniqueChildContext, category.Typed);
                Dump("result1", result1);
                BreakExecution();
                var result = result1
                    .SmartUn<FunctionType>();
                Dump("result", result);
                return ReturnMethodDump(result.AutomaticDereferenceResult);
            }
            finally
            {
                EndMethodDump();
            }
        }

        bool InternalInnerHllw(ContextBase parent, int position)
        {
            var uniqueChildContext = parent
                .UniqueStructurePositionContext(this, position);
            return Statements[position].HllwStructureElement(uniqueChildContext);
        }

        bool? InternalInnerHllw(int position) { return Statements[position].Hllw; }

        internal bool ObtainHllw(ContextBase parent, int accessPosition)
        {
            var trace = ObjectId == -10 && accessPosition == 3 && parent.ObjectId == 4;
            StartMethodDump(trace, parent, accessPosition);
            try
            {
                var subStatementIds = accessPosition.Select().ToArray();
                Dump("subStatementIds", subStatementIds);
                BreakExecution();
                if(subStatementIds.Any(position => InternalInnerHllw(position) == false))
                    return ReturnMethodDump(false);
                var quickNonDataLess = subStatementIds
                    .Where(position => InternalInnerHllw(position) == null)
                    .ToArray();
                Dump("quickNonDataLess", quickNonDataLess);
                BreakExecution();
                if(quickNonDataLess.Length == 0)
                    return ReturnMethodDump(true);
                if(quickNonDataLess.Any(position => InternalInnerHllw(parent, position) == false))
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
            if(category.IsNone)
                return new Result();
            var trace = ObjectId == -1 && category.HasCode;
            StartMethodDump(trace, category, parent, fromPosition, fromNotPosition);
            try
            {
                Dump("Statements", Statements);

                var enumerable = (fromNotPosition - fromPosition)
                    .Select(i => fromPosition + i)
                    .Where(position => !Statements[position].IsLambda)
                    .ToArray();
                Dump("Statements", enumerable);
                BreakExecution();
                var @select = enumerable
                    .Select(position => InnerResult(category, parent, position))
                    .Select(r => r.Align)
                    .ToArray();
                Dump("Statements", select);
                BreakExecution();
                var results = @select
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

        internal Size StructureSize(ContextBase parent, int fromPosition, int fromNotPosition)
        {
            return StructureResult(Category.Size, parent, fromPosition, fromNotPosition).Size;
        }

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
                .Select()
                .Where(i => !InternalInnerHllw(parent, i))
                .Select(i => i.ToString() + "=" + InnerResult(Category.Size, parent, i).Size.ToString())
                .ToArray();
        }
    }


    sealed class StructurePosition : DumpableObject
    {
        [EnableDump]
        readonly int _position;

        internal StructurePosition(int position) { _position = position; }

        internal AccessFeature Convert(Structure accessPoint) { return accessPoint.UniqueAccessFeature(_position); }
    }
}