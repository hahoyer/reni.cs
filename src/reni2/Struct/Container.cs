using System.Collections.Generic;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using HWClassLibrary.Helper.TreeViewSupport;
using HWClassLibrary.IO;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Parser.TokenClass;
using Reni.Syntax;
using Reni.Type;

namespace Reni.Struct
{
    /// <summary>
    /// Structured data, context free version
    /// </summary>
    internal sealed class Container : ReniObject, IDumpShortProvider
    {
        private static readonly string _runId = Compiler.FormattedNow + "\n";
        public static bool _isInDump;
        private static bool _isInsideFileDump;
        private static int _nextObjectId;
        private readonly List<SyntaxBase> _converterList;
        private readonly DictionaryEx<string, int> _dictionary;
        private readonly List<SyntaxBase> _list;
        private DictionaryEx<int, string> _reverseDictionaryCache;
        private IStructFeature[] _structFeaturesCache;

        private Container
            (
            List<SyntaxBase> list,
            List<SyntaxBase> converterList,
            DictionaryEx<string, int> dict
            )
            : base(_nextObjectId++)
        {
            _list = list;
            foreach(var elem in _list)
                Tracer.Assert(!(elem is Syntax.Struct));

            _converterList = converterList;
            _dictionary = dict;
        }

        private Container(List<SyntaxBase> list)
            : this(list, new List<SyntaxBase>(), new DictionaryEx<string, int>()) {}

        private Container(List<SyntaxBase> list, DictionaryEx<string, int> dictionary)
            : this(list, new List<SyntaxBase>(), dictionary) {}

        [Node]
        public List<SyntaxBase> List { get { return _list; } }

        [Node]
        public List<SyntaxBase> ConverterList { get { return _converterList; } }

        [Node, DumpData(false)]
        public DictionaryEx<int, string> ReverseDictionary
        {
            get
            {
                if(_reverseDictionaryCache == null)
                    CreateReverseDictionary();
                return _reverseDictionaryCache;
            }
        }

        [Node, DumpData(false)]
        private IStructFeature[] StructFeatures
        {
            get
            {
                if(_structFeaturesCache == null)
                    _structFeaturesCache = CreateStructContainerFeatures();
                return _structFeaturesCache;
            }
        }

        [Node]
        public DictionaryEx<string, int> Dictionary { get { return _dictionary; } }

        public SyntaxBase this[int index] { get { return _list[index]; } }

        [DumpData(false)]
// ReSharper disable MemberCanBeMadeStatic
            internal TypeBase IndexType
// ReSharper restore MemberCanBeMadeStatic
        {
            get
            {
                return MaxIndexType;
                //return Type.Base.CreateNumber(BitsConst.Convert(_list.Count).Size.ToInt());
            }
        }

        internal static TypeBase MaxIndexType { get { return TypeBase.CreateNumber(32); } }
        public string FilePosition { get { return _list[0].FilePosition; } }

        public string DumpShort()
        {
            return "Container." + ObjectId;
        }

        private IStructFeature[] CreateStructContainerFeatures()
        {
            var result = new List<IStructFeature>();
            for(var i = 0; i < _list.Count; i++)
                result.Add(new StructFeature(i));
            return result.ToArray();
        }

        private void CreateReverseDictionary()
        {
            _reverseDictionaryCache = new DictionaryEx<int, string>();
            foreach(var pair in _dictionary)
                _reverseDictionaryCache[pair.Value] = pair.Key;
        }

        internal static Container Create(SyntaxBase left)
        {
            var list =
                new List<SyntaxBase> {left};
            return new Container(list);
        }

        internal static Container Create(SyntaxBase left, SyntaxBase right)
        {
            return new Container(new List<SyntaxBase> {left, right});
        }

        internal static Container Create(DeclarationSyntax left, SyntaxBase right)
        {
            var list = new List<SyntaxBase> {left.Definition, right};
            var dictionary = new DictionaryEx<string, int>();
            dictionary[left.DefineableToken.Name] = 0;
            return new Container(list, dictionary);
        }

        internal static Container Create(DeclarationSyntax left)
        {
            var list = new List<SyntaxBase> {left.Definition};
            var dictionary = new DictionaryEx<string, int>();
            dictionary[left.DefineableToken.Name] = 0;
            return new Container(list, dictionary);
        }

        internal static Container Create(ConverterSyntax left)
        {
            var list = new List<SyntaxBase>();
            var converter = new List<SyntaxBase> {left.Body};
            var dictionary = new DictionaryEx<string, int>();
            return new Container(list, converter, dictionary);
        }

        internal static Container Create(SyntaxBase left, Container right)
        {
            var list = new List<SyntaxBase> {left};
            list.AddRange(right._list);
            var dictionary = new DictionaryEx<string, int>();
            foreach(var pair in right._dictionary)
                dictionary[pair.Key] = pair.Value + 1;

            return new Container(list, right._converterList, dictionary);
        }

        internal static Container Create(DeclarationSyntax left, Container right)
        {
            var list = new List<SyntaxBase> {left.Definition};
            list.AddRange(right._list);
            var dictionary = new DictionaryEx<string, int>();
            dictionary[left.DefineableToken.Name] = 0;
            foreach(var pair in right._dictionary)
                dictionary[pair.Key] = pair.Value + 1;

            return new Container(list, right._converterList, dictionary);
        }

        public string DumpPrintText(ContextBase context)
        {
            var result = "";
            for(var i = 0; i < _list.Count; i++)
            {
                if(i > 0)
                    result += ";";
                if(ReverseDictionary.ContainsKey(i))
                    result += ReverseDictionary[i] + ": ";
                result += _list[i].VisitType(context);
            }
            return result;
        }

        public override string DumpData()
        {
            var isInsideFileDump = _isInsideFileDump;
            _isInsideFileDump = true;
            var result = isInsideFileDump ? DumpDataToString() : DumpDataToFile();
            _isInsideFileDump = isInsideFileDump;
            return result;
        }

        private string DumpDataToFile()
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

        private string DumpDataToString()
        {
            var isInDump = _isInDump;
            _isInDump = true;
            var result = base.DumpData();
            _isInDump = isInDump;
            return result;
        }

        public Result Visit(ContextBase context, Category category, int fromNotPosition)
        {
            var trace =
                ObjectId == 4 &&
                    context.ObjectId == 4 &&
                        category.ToString() == "Size,Type,Refs,Code";
            //trace = false;
            StartMethodDumpWithBreak(trace, context, category, fromNotPosition);
            var result = TypeBase.CreateVoidResult(category);
            var topRef = context.CreateTopRefCode();
            for(var i = 0; i < fromNotPosition; i++)
            {
                ContextBase structContext =
                    context.CreateStructAtPosition(this, i);
                var rawResult =
                    _list[i].Visit(structContext, category | Category.Type);
                var iresult =
                    rawResult.Align(structContext.RefAlignParam.AlignBits);
                if(iresult.IsPending)
                    return ReturnMethodDump(trace, iresult);
                if(trace)
                    DumpDataWithBreak("", "i", i, "result", result, "iresult", iresult);
                result.Add(iresult);
            }
            if(category.HasType)
                result.Type = CreateStructType(context, fromNotPosition);
            var resultReplaced
                = result.ReplaceRelativeContextRef(CreateContext(context), topRef);
            return ReturnMethodDump(trace, resultReplaced);
        }

        private Result VisitElement(ContextBase context, Category category, int index)
        {
            var trace = ObjectId == -1;
            StartMethodDumpWithBreak(trace, context, category, index);
            ContextBase structContext = context.CreateStructAtPosition(this, index);
            var rawResult =
                _list[index].Visit(structContext, category | Category.Type);
            var iresult = rawResult.PostProcess(structContext.RefAlignParam);
            if(trace)
                DumpDataWithBreak("", "rawResult", rawResult, "iresult", iresult);
            if(iresult.IsPending)
                return ReturnMethodDump(trace, iresult);
            var topRef =
                context
                    .CreateTopRefCode()
                    .CreateRefPlus(context.RefAlignParam, VisitSize(context));
            var resultReplaced =
                iresult
                    .ReplaceRelativeContextRef(CreateContext(context), topRef);
            return ReturnMethodDump(trace, resultReplaced);
        }

        internal Type CreateStructType(ContextBase context, int currentCompilePosition)
        {
            return CreateContext(context).CreateStructType(currentCompilePosition);
        }

        internal Context CreateContext(ContextBase context)
        {
            return context.CreateStructContext(this);
        }

        /// <summary>
        /// Visits the element at position. Result will be a reference or a function.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="category">The category.</param>
        /// <param name="position">The position.</param>
        /// <returns></returns>
        /// <remarks>
        /// The context used is bound to compiled position
        /// </remarks>
        /// created 16.12.2006 13:15
        public Result VisitElementFromContextRef(ContextBase context, Category category, int position)
        {
            return context.CreateStructContext(this).VisitElementFromContextRef(category, position);
        }

        public Result DestructorHandler(ContextBase context, Category category, Result objRefResult,
            int accessPosition)
        {
            var result = TypeBase.CreateVoidResult(category - Category.Type - Category.Size);
            var size = VisitSize(context, accessPosition);
            for(var i = 0; i < accessPosition; i++)
            {
                var iType = VisitElementTypeFromContextRef(context, i);
                size -= iType.Size;
                result.Add(
                    iType.DestructorHandler(category).UseWithArg(
                        objRefResult.CreateRefPlus(category, context.RefAlignParam, size)));
            }
            return result;
        }

        public int Find(string name)
        {
            return _dictionary[name];
        }

        public bool Defined(string name)
        {
            return _dictionary.ContainsKey(name);
        }

        public Size VisitSize(ContextBase context, int fromNotPosition)
        {
            return Visit(context, Category.Size, fromNotPosition).Size;
        }

        private Size VisitSize(ContextBase context)
        {
            return VisitSize(context, _list.Count);
        }

        public TypeBase VisitType(ContextBase context, int currentCompilePosition)
        {
            return Visit(context, Category.Type, currentCompilePosition).Type;
        }

        public Result Visit(ContextBase context, Category category)
        {
            return Visit(context, category, _list.Count);
        }

        public TypeBase VisitElementTypeFromContextRef(ContextBase context, int index)
        {
            return VisitElementFromContextRef(context, Category.Type, index).Type;
        }

        public Result DumpPrintFromRef(Category category, ContextBase context, RefAlignParam refAlignParam)
        {
            Tracer.Assert(refAlignParam.Equals(context.RefAlignParam));
            const string dumpPrintName = "dump_print";
            if(Defined(dumpPrintName))
                return VisitElement(context, category, Find(dumpPrintName));
            var result = DumpPrintFromRef(category, context);
            return Result.ConcatPrintResult(category, result);
        }

        private List<Result> DumpPrintFromRef(Category category, ContextBase context)
        {
            var result = new List<Result>();
            var containerContext = CreateContext(context);
            for(var i = 0; i < _list.Count; i++)
            {
                var iResult = VisitElementTypeFromContextRef(context, i).DumpPrint(category);
                var iAccess = containerContext.CreateStructType(_list.Count).AccessFromArg(category, i);
                iResult = iResult.UseWithArg(iAccess);
                result.Add(iResult);
            }
            return result;
        }

        public int? IndexOfConverterTo(ContextBase context, TypeBase dest)
        {
            var structContext = CreateContext(context);
            for(var i = 0; i < _converterList.Count; i++)
                if(
                    _converterList[i]
                        .VisitType(structContext)
                        .IsConvertableTo(dest, ConversionFeature.Instance.DontUseConverter)
                    )
                {
                    for(var ii = i + 1; ii < _converterList.Count; ii++)
                        Tracer.Assert(
                            !_converterList[ii]
                                .VisitType(structContext)
                                .IsConvertableTo(dest, ConversionFeature.Instance.DontUseConverter));
                    return i;
                }
            return null;
        }

        public bool HasConverterTo(ContextBase context, TypeBase dest)
        {
            return IndexOfConverterTo(context, dest) != null;
        }

        public bool IsConvertableToVoid(ContextBase context)
        {
            for(var i = 0; i < _list.Count; i++)
                if(!_list[i].VisitType(context).IsVoid)
                    return false;
            return true;
        }

        public bool IsPendingType(ContextBase context)
        {
            for(var i = 0; i < _list.Count; i++)
            {
                ContextBase structContext = context.CreateStructAtPosition(this, i);
                if(_list[i].VisitType(structContext).IsPending)
                    return true;
            }
            return false;
        }

        internal Result ConvertTo(Category category, ContextBase context, TypeBase dest)
        {
            // Special case if dest is Void and size is zero
            if(dest is Reni.Type.Void && VisitSize(context, _list.Count).IsZero)
                return TypeBase.CreateVoid.CreateArgResult(category);

            return _converterList[IndexOfConverterTo(context, dest).Value]
                .Visit(CreateContext(context), category);
        }

        internal Size BackOffset(ContextBase context, int index)
        {
            return VisitSize(context, index + 1)*-1;
        }

        internal Size Offset(ContextBase context, int index)
        {
            return VisitSize(context, _list.Count) - VisitSize(context, index);
        }

        internal CodeBase CreateRef(ContextBase parent)
        {
            return parent.CreateTopRefCode().CreateRefPlus(parent.RefAlignParam, Offset(parent, 0));
        }

        public Result MoveHandler(Category category, ContextBase context, int currentCompilePosition)
        {
            for(var i = 0; i < _list.Count; i++)
            {
                var iResult = _list[i].VisitType(context).MoveHandler(category);
                if(!iResult.IsEmpty)
                    NotImplementedMethod(category, context, currentCompilePosition, "i", i, "iResult", iResult);
            }
            return TypeBase.EmptyHandler(category);
        }

        public Result VisitOperationApply(ContextBase definingParentContext, ContextBase callContext,
            Category category, SyntaxBase args)
        {
            var indexValue = args.VisitAndEvaluate(callContext, IndexType);
            var index = indexValue.ToInt32();
            return VisitElementFromContextRef(definingParentContext, category, index);
        }

        internal Result VisitAccessApply(ContextBase structContext, int position, ContextBase callContext,
            Category category,
            SyntaxBase args)
        {
            var localCategory = category;
            if(args != null)
                localCategory = localCategory | Category.Type;
            var functionResult = VisitElementFromContextRef(structContext, localCategory, position);
            if(args == null)
                return functionResult;

            if(functionResult.IsCodeLess)
                return functionResult.Type.ApplyFunction(category, callContext, args);

            NotImplementedMethod(position, callContext, category, args);
            return null;
        }

        internal SearchResult<IStructFeature> Search(Defineable defineable)
        {
            if(Defined(defineable.Name))
                return SearchResult<IStructFeature>.Success(StructFeatures[Find(defineable.Name)],
                    defineable);
            return defineable.SearchFromStruct().SubTrial(this);
        }

        internal class AtFeature : IContextFeature, IFeature
        {
            private readonly Container _container;
            private readonly ContextBase _parentContext;

            public AtFeature(Container container, ContextBase parentContext)
            {
                _parentContext = parentContext;
                _container = container;
            }

            public Result VisitApply(ContextBase callContext, Category category, SyntaxBase args)
            {
                return _container.VisitOperationApply(_parentContext, callContext, category, args);
            }

            public Result VisitApply(ContextBase callContext, Category category, SyntaxBase args, Ref callObject)
            {
                return _container.VisitOperationApply(_parentContext, callContext, category, args);
            }
        }

        internal class ContextFeature : IContextFeature
        {
            [DumpData(true)]
            private readonly Container _container;
            [DumpData(true)]
            private readonly int _index;
            private readonly ContextBase _parentContext;

            public ContextFeature(Container container, ContextBase parentContext, int index)
            {
                _parentContext = parentContext;
                _container = container;
                _index = index;
            }

            public Result VisitApply(ContextBase contextBase, Category category, SyntaxBase args)
            {
                return _container.VisitAccessApply(_parentContext, _index, contextBase, category, args);
            }
        }

        internal class StructFeature : IStructFeature
        {
            private readonly int _index;

            public StructFeature(int index)
            {
                _index = index;
            }

            public IContextFeature Convert(Context context)
            {
                return context.CreateMemberAccess(_index);
            }

            public IFeature Convert(Type type)
            {
                return type.CreateMemberAccess(_index);
            }
        }

        internal AtFeature AtFeatureObject(ContextBase parent)
        {
            return new AtFeature(this, parent);
        }

        internal IContextFeature[] CreateContextFeaturesCache(ContextBase Parent)
        {
            var result = new List<ContextFeature>();
            for(var i = 0; i < List.Count; i++)
                result.Add(new ContextFeature(this, Parent, i));
            return result.ToArray();
        }
    }
}