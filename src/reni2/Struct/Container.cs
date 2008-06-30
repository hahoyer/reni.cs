using System.Collections.Generic;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using HWClassLibrary.Helper.TreeViewSupport;
using HWClassLibrary.IO;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Parser;
using Reni.Parser.TokenClass;
using Reni.Syntax;
using Reni.Type;

namespace Reni.Struct
{
    /// <summary>
    /// Structured data, context free version
    /// </summary>
    internal sealed class Container : ParsedSyntax, IDumpShortProvider, ICompileSyntax
    {
        private static readonly string _runId = Compiler.FormattedNow + "\n";
        public static bool _isInDump; //TODO: Move this variable to ParsedSyntax
        private static bool _isInsideFileDump;
        private static int _nextObjectId;
        internal readonly List<ICompileSyntax> ConverterList = new List<ICompileSyntax>();
        internal readonly DictionaryEx<string, int> Dictionary = new DictionaryEx<string, int>();
        internal readonly List<ICompileSyntax> List = new List<ICompileSyntax>();
        private DictionaryEx<int, string> _reverseDictionaryCache;
        private IStructFeature[] _structFeaturesCache;

        private Container(Token token) : base(token, _nextObjectId++) {}

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

        public ICompileSyntax this[int index] { get { return List[index]; } }

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

        Result ICompileSyntax.Result(ContextBase context, Category category)
        {
            var result = Result(context, category - Category.Internal);
            if (category.HasInternal)
                result.Internal = Reni.Type.Void.CreateResult(Category.ForInternal);
            return result;
        }

        Result Result(ContextBase context, Category category)
        {
            return PartialResult(context, category, List.Count);
        }

        public Size Size(ContextBase context)
        {
            return Result(context, Category.Size).Size;
        }

        public TypeBase Type(ContextBase context)
        {
            return Result(context, Category.Type).Type;
        }

        string ICompileSyntax.DumpShort()
        {
            return DumpShort();
        }

        string ICompileSyntax.FilePosition()
        {
            return FilePosition();
        }

        internal protected override string DumpShort()
        {
            return "container." + ObjectId;
        }

        private IStructFeature[] CreateStructContainerFeatures()
        {
            var result = new List<IStructFeature>();
            for(var i = 0; i < List.Count; i++)
                result.Add(new StructFeature(i));
            return result.ToArray();
        }

        private void CreateReverseDictionary()
        {
            _reverseDictionaryCache = new DictionaryEx<int, string>();
            foreach(var pair in Dictionary)
                _reverseDictionaryCache[pair.Value] = pair.Key;
        }

        internal static IParsedSyntax Create(Token token, List<IParsedSyntax> parsed)
        {
            var result = new Container(token);
            foreach(var parsedSyntax in parsed)
                result.Add(parsedSyntax);
            return result;
        }

        internal static IParsedSyntax Create(Token token, IParsedSyntax parsedSyntax)
        {
            var result = new Container(token);
            result.Add(parsedSyntax);
            return result;
        }

        private void Add(IParsedSyntax parsedSyntax)
        {
            while(parsedSyntax is DeclarationSyntax)
            {
                var d = (DeclarationSyntax) parsedSyntax;
                Dictionary.Add(d.Name.Name, List.Count);
                parsedSyntax = d.Definition;
            }
            if(parsedSyntax is ConverterSyntax)
                ConverterList.Add(((ConverterSyntax) parsedSyntax).Body);
            else
                List.Add(parsedSyntax.CompileSyntax);
        }

        public string DumpPrintText(ContextBase context)
        {
            var result = "";
            for(var i = 0; i < List.Count; i++)
            {
                if(i > 0)
                    result += ";";
                if(ReverseDictionary.ContainsKey(i))
                    result += ReverseDictionary[i] + ": ";
                result += context.Type(List[i]);
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

        internal Result PartialResult(ContextBase context, Category category, int fromNotPosition)
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
                    structContext.Result(category | Category.Type, List[i]);
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

        private Result ElementResult(ContextBase context, Category category, int index)
        {
            var trace = ObjectId == -1;
            StartMethodDumpWithBreak(trace, context, category, index);
            ContextBase structContext = context.CreateStructAtPosition(this, index);
            var rawResult = structContext.Result(category | Category.Type, List[index]);
            var iresult = rawResult.PostProcess(context.RefAlignParam);
            if(trace)
                DumpDataWithBreak("", "rawResult", rawResult, "iresult", iresult);
            if(iresult.IsPending)
                return ReturnMethodDump(trace, iresult);
            var topRef =
                context
                    .CreateTopRefCode()
                    .CreateRefPlus(context.RefAlignParam, Size(context));
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
            var size = PartialSize(context, accessPosition);
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
            return Dictionary[name];
        }

        public bool Defined(string name)
        {
            return Dictionary.ContainsKey(name);
        }

        public Size PartialSize(ContextBase context, int fromNotPosition)
        {
            return PartialResult(context, Category.Size, fromNotPosition).Size;
        }

        public TypeBase VisitType(ContextBase context, int currentCompilePosition)
        {
            return PartialResult(context, Category.Type, currentCompilePosition).Type;
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
                return ElementResult(context, category, Find(dumpPrintName));
            var result = DumpPrintFromRef(category, context);
            return Reni.Result.ConcatPrintResult(category, result);
        }

        private List<Result> DumpPrintFromRef(Category category, ContextBase context)
        {
            var result = new List<Result>();
            var containerContext = CreateContext(context);
            for(var i = 0; i < List.Count; i++)
            {
                var iResult = VisitElementTypeFromContextRef(context, i).DumpPrint(category);
                var iAccess = containerContext.CreateStructType(List.Count).AccessFromArg(category, i);
                iResult = iResult.UseWithArg(iAccess);
                result.Add(iResult);
            }
            return result;
        }

        public int? IndexOfConverterTo(ContextBase context, TypeBase dest)
        {
            var structContext = CreateContext(context);
            for(var i = 0; i < ConverterList.Count; i++)
                if(
                    structContext
                        .Type(ConverterList[i])
                        .IsConvertableTo(dest, ConversionFeature.Instance.DontUseConverter)
                    )
                {
                    for(var ii = i + 1; ii < ConverterList.Count; ii++)
                        Tracer.Assert(
                            !structContext
                                .Type(ConverterList[i])
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
            for(var i = 0; i < List.Count; i++)
                if(!context.Type(List[i]).IsVoid)
                    return false;
            return true;
        }

        public bool IsPendingType(ContextBase context)
        {
            for(var i = 0; i < List.Count; i++)
            {
                ContextBase structContext = context.CreateStructAtPosition(this, i);
                if(structContext.Type(List[i]).IsPending)
                    return true;
            }
            return false;
        }

        internal Result ConvertTo(Category category, ContextBase context, TypeBase dest)
        {
            // Special case if dest is Void and size is zero
            if(dest is Reni.Type.Void && Size(context).IsZero)
                return TypeBase.CreateVoid.CreateArgResult(category);

            return CreateContext(context)
                .Result(category, ConverterList[IndexOfConverterTo(context, dest).Value]);
        }

        internal Size BackOffset(ContextBase context, int index)
        {
            return PartialSize(context, index + 1)*-1;
        }

        internal Size Offset(ContextBase context, int index)
        {
            return Size(context) - PartialSize(context, index);
        }

        internal CodeBase CreateRef(ContextBase parent)
        {
            return parent.CreateTopRefCode().CreateRefPlus(parent.RefAlignParam, Offset(parent, 0));
        }

        public Result MoveHandler(Category category, ContextBase context, int currentCompilePosition)
        {
            for(var i = 0; i < List.Count; i++)
            {
                var iResult = context.Type(List[i]).MoveHandler(category);
                if(!iResult.IsEmpty)
                    NotImplementedMethod(category, context, currentCompilePosition, "i", i, "iResult", iResult);
            }
            return TypeBase.EmptyHandler(category);
        }

        public Result VisitOperationApply(ContextBase definingParentContext, ContextBase callContext, Category category, ICompileSyntax args)
        {
            var indexValue = callContext.Evaluate(args, IndexType);
            var index = indexValue.ToInt32();
            return VisitElementFromContextRef(definingParentContext, category, index);
        }

        internal Result ApplyAtFeature(ContextBase context, ContextBase callContext, Category category, ICompileSyntax @object, ICompileSyntax args)
        {
            var index = callContext.Evaluate(args, IndexType).ToInt32();
            return AccessApplyResult(context, index, callContext, category, @object);
        }

        internal Result VisitAccessApply(ContextBase structContext, int position, ContextBase callContext, Category category, ICompileSyntax args)
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

        internal Result AccessApplyResult(ContextBase context, int index, ContextBase callContext, Category category, ICompileSyntax @object, ICompileSyntax args)
        {
            NotImplementedMethod(context,index,callContext,category,@object,args);
            return null;
        }

        internal Result AccessApplyResult(ContextBase context, int index, ContextBase callContext, Category category, ICompileSyntax @object)
        {
            NotImplementedMethod(context, index, callContext, category, @object);
            return null;
        }

        internal SearchResult<IStructFeature> Search(Defineable defineable)
        {
            if(Defined(defineable.Name))
                return SearchResult<IStructFeature>.Success(StructFeatures[Find(defineable.Name)],
                    defineable);
            return defineable.SearchFromStruct().SubTrial(this);
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

            public Result VisitApply(ContextBase contextBase, Category category, ICompileSyntax args)
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

        string IDumpShortProvider.DumpShort()
        {
            return DumpShort();
        }

    }
}