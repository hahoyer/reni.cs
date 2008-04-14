using System.Collections.Generic;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using HWClassLibrary.Helper.TreeViewSupport;
using HWClassLibrary.IO;
using Reni.Context;
using Reni.Parser;
using Reni.Parser.TokenClass;
using Reni.Syntax;
using Reni.Type;

namespace Reni.Struct
{
    /// <summary>
    /// Structured data, context free version
    /// </summary>
    internal sealed class Container : ReniObject
    {
        private static readonly string _runId = Compiler.FormattedNow + "\n";
        public static bool _isInDump;
        private static bool _isInsideFileDump;
        private static int _nextObjectId;
        private readonly List<Syntax.Base> _converterList;
        private readonly DictionaryEx<string, int> _dictionary;
        private readonly List<Syntax.Base> _list;
        private DictionaryEx<int, string> _rdict;

        private Container
            (
            List<Syntax.Base> list,
            List<Syntax.Base> converterList,
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

        private Container(List<Syntax.Base> list)
            : this(list, new List<Syntax.Base>(), new DictionaryEx<string, int>()) {}

        private Container(List<Syntax.Base> list, DictionaryEx<string, int> dictionary)
            : this(list, new List<Syntax.Base>(), dictionary) {}

        [Node]
        public List<Syntax.Base> List { get { return _list; } }

        [Node]
        public List<Syntax.Base> ConverterList { get { return _converterList; } }

        [Node, DumpData(false)]
        public DictionaryEx<int, string> Rdict
        {
            get
            {
                if(_rdict == null)
                    CreateRDict();
                return _rdict;
            }
        }

        [Node]
        public DictionaryEx<string, int> Dictionary { get { return _dictionary; } }

        /// <summary>
        /// indexer
        /// </summary>
        /// <value></value>
        /// created 15.12.2006 23:55
        public Syntax.Base this[int index] { get { return _list[index]; } }

        [DumpData(false)]
// ReSharper disable MemberCanBeMadeStatic
        internal Reni.Type.Base IndexType
// ReSharper restore MemberCanBeMadeStatic
        {
            get
            {
                return MaxIndexType;
                //return Type.Base.CreateNumber(BitsConst.Convert(_list.Count).Size.ToInt());
            }
        }

        internal static Reni.Type.Base MaxIndexType { get { return Reni.Type.Base.CreateNumber(32); } }

        private void CreateRDict()
        {
            _rdict = new DictionaryEx<int, string>();
            foreach(var pair in _dictionary)
                _rdict[pair.Value] = pair.Key;
        }

        internal static Container Create(Syntax.Base left)
        {
            var list =
                new List<Syntax.Base> {left};
            return new Container(list);
        }

        internal static Container Create(Syntax.Base left, Syntax.Base right)
        {
            return new Container(new List<Syntax.Base> {left, right});
        }

        internal static Container Create(DeclarationSyntax left, Syntax.Base right)
        {
            var list = new List<Syntax.Base> {left.Definition, right};
            var dictionary = new DictionaryEx<string, int>();
            dictionary[left.DefineableToken.Name] = 0;
            return new Container(list, dictionary);
        }

        internal static Container Create(DeclarationSyntax left)
        {
            var list = new List<Syntax.Base> {left.Definition};
            var dictionary = new DictionaryEx<string, int>();
            dictionary[left.DefineableToken.Name] = 0;
            return new Container(list, dictionary);
        }

        internal static Container Create(ConverterSyntax left)
        {
            var list = new List<Syntax.Base>();
            var converter = new List<Syntax.Base> {left.Body};
            var dictionary = new DictionaryEx<string, int>();
            return new Container(list, converter, dictionary);
        }

        internal static Container Create(Syntax.Base left, Container right)
        {
            var list = new List<Syntax.Base> {left};
            list.AddRange(right._list);
            var dictionary = new DictionaryEx<string, int>();
            foreach(var pair in right._dictionary)
                dictionary[pair.Key] = pair.Value + 1;

            return new Container(list, right._converterList, dictionary);
        }

        internal static Container Create(DeclarationSyntax left, Container right)
        {
            var list = new List<Syntax.Base> {left.Definition};
            list.AddRange(right._list);
            var dictionary = new DictionaryEx<string, int>();
            dictionary[left.DefineableToken.Name] = 0;
            foreach(var pair in right._dictionary)
                dictionary[pair.Key] = pair.Value + 1;

            return new Container(list, right._converterList, dictionary);
        }

        /// <summary>
        /// Gets the dump print text.
        /// </summary>
        /// <value>The dump print text.</value>
        /// created 14.01.2007 03:23
        public string DumpPrintText(Reni.Context.Base context)
        {
            var result = "";
            for(var i = 0; i < _list.Count; i++)
            {
                if(i > 0)
                    result += ";";
                if(_rdict.ContainsKey(i))
                    result += _rdict[i] + ": ";
                result += _list[i].VisitType(context);
            }
            return result;
        }

        /// <summary>
        /// Default dump of data
        /// </summary>
        /// <returns></returns>
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
            var newResult = _runId + DumpDataToString();
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

        /// <summary>
        /// Visitor function
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="category">The category.</param>
        /// <param name="fromNotPosition">From not position.</param>
        /// <returns></returns>
        public Result Visit(Reni.Context.Base context, Category category, int fromNotPosition)
        {
            var trace =
                ObjectId == 4 &&
                    context.ObjectId == 4 &&
                        category.ToString() == "Size,Type,Refs,Code";
            //trace = false;
            StartMethodDumpWithBreak(trace, context, category, fromNotPosition);
            var result = Reni.Type.Base.CreateVoidResult(category);
            var topRef = context.CreateTopRefCode();
            for(var i = 0; i < fromNotPosition; i++)
            {
                Reni.Context.Base structContext = 
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

        private Result VisitElement(Reni.Context.Base context, Category category, int index)
        {
            var trace = ObjectId == -1;
            StartMethodDumpWithBreak(trace, context, category, index);
            Reni.Context.Base structContext = context.CreateStructAtPosition(this, index);
            var rawResult = 
                _list[index].Visit(structContext, category | Category.Type);
            var iresult = rawResult.PostProcess(structContext);
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

        internal Type CreateStructType(Reni.Context.Base context, int currentCompilePosition)
        {
            return CreateContext(context).CreateStructType(currentCompilePosition);
        }

        internal Context CreateContext(Reni.Context.Base context)
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
        public Result VisitElementFromContextRef(Reni.Context.Base context, Category category, int position)
        {
            return context
                .CreateStructAtPosition(this, position)
                .VisitElementFromContextRef(category, position);
        }

        /// <summary>
        /// Destructors the handler.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="category">The category.</param>
        /// <param name="objRefResult">Reference to object.</param>
        /// <param name="accessPosition">The access position.</param>
        /// <returns></returns>
        /// created 10.12.2006 15:34
        public Result DestructorHandler(Reni.Context.Base context, Category category, Result objRefResult,
            int accessPosition)
        {
            var result = Reni.Type.Base.CreateVoidResult(category - Category.Type - Category.Size);
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

        /// <summary>
        /// Finds the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        /// [created 09.06.2006 21:26]
        public int Find(string name)
        {
            return _dictionary[name];
        }

        /// <summary>
        /// Defineses the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        /// [created 10.06.2006 10:13]
        public bool Defines(string name)
        {
            return _dictionary.ContainsKey(name);
        }

        /// <summary>
        /// Visits the size.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="fromNotPosition">From not position.</param>
        /// <returns></returns>
        /// created 09.12.2006 00:33
        public Size VisitSize(Reni.Context.Base context, int fromNotPosition)
        {
            return Visit(context, Category.Size, fromNotPosition).Size;
        }

        private Size VisitSize(Reni.Context.Base context)
        {
            return VisitSize(context, _list.Count);
        }

        /// <summary>
        /// Visits the type.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="currentCompilePosition">The current compile position.</param>
        /// <returns></returns>
        /// created 12.12.2006 23:27
        public Reni.Type.Base VisitType(Reni.Context.Base context, int currentCompilePosition)
        {
            return Visit(context, Category.Type, currentCompilePosition).Type;
        }

        /// <summary>
        /// Visits the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="category">The category.</param>
        /// <returns></returns>
        /// created 13.12.2006 00:01
        public Result Visit(Reni.Context.Base context, Category category)
        {
            return Visit(context, category, _list.Count);
        }

        /// <summary>
        /// Visits the type of the element.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        /// created 15.12.2006 23:49
        public Reni.Type.Base VisitElementTypeFromContextRef(Reni.Context.Base context, int index)
        {
            return VisitElementFromContextRef(context, Category.Type, index).Type;
        }

        /// <summary>
        /// Dumps the print code.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="context">The context.</param>
        /// <param name="refAlignParam">The ref align param.</param>
        /// <returns></returns>
        /// created 30.01.2007 23:31
        public Result DumpPrintFromRef(Category category, Reni.Context.Base context, RefAlignParam refAlignParam)
        {
            Tracer.Assert(refAlignParam.Equals(context.RefAlignParam));
            const string dumpPrintName = "dump_print";
            if(Defines(dumpPrintName))
                return VisitElement(context, category, Find(dumpPrintName));
            var result = DumpPrintFromRef(category, context);
            return Result.ConcatPrintResult(category, result);
        }

        private List<Result> DumpPrintFromRef(Category category, Reni.Context.Base context)
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

        public int? IndexOfConverterTo(Reni.Context.Base context, Reni.Type.Base dest)
        {
            var structContext = CreateContext(context);
            for(var i = 0; i < _converterList.Count; i++)
            {
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
            }
            return null;
        }

        public bool HasConverterTo(Reni.Context.Base context, Reni.Type.Base dest)
        {
            return IndexOfConverterTo(context, dest) != null;
        }

        /// <summary>
        /// Determines whether [is convertable to void] [the specified context].
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>
        /// 	<c>true</c> if [is convertable to void] [the specified context]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsConvertableToVoid(Reni.Context.Base context)
        {
            for(var i = 0; i < _list.Count; i++)
            {
                if(!_list[i].VisitType(context).IsVoid)
                    return false;
            }
            return true;
        }

        public bool IsPendingType(Reni.Context.Base context)
        {
            for(var i = 0; i < _list.Count; i++)
            {
                Reni.Context.Base structContext = context.CreateStructAtPosition(this, i);
                if(_list[i].VisitType(structContext).IsPending)
                    return true;
            }
            return false;
        }

        internal Result ConvertTo(Category category, Reni.Context.Base context, Reni.Type.Base dest)
        {
            // Special case if dest is Void and size is zero
            if(dest is Reni.Type.Void && VisitSize(context, _list.Count).IsZero)
                return Reni.Type.Base.CreateVoid.CreateArgResult(category);

            return _converterList[IndexOfConverterTo(context, dest).Value]
                .Visit(CreateContext(context), category);
        }

        /// <summary>
        /// Offsets the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        internal Size BackOffset(Reni.Context.Base context, int index)
        {
            return VisitSize(context, index + 1)*-1;
        }

        /// <summary>
        /// Offsets the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        internal Size Offset(Reni.Context.Base context, int index)
        {
            return VisitSize(context, _list.Count) - VisitSize(context, index);
        }

        internal Code.Base CreateRef(Reni.Context.Base parent)
        {
            return parent.CreateTopRefCode().CreateRefPlus(parent.RefAlignParam, Offset(parent, 0));
        }

        public Result MoveHandler(Category category, Reni.Context.Base context, int currentCompilePosition)
        {
            for(var i = 0; i < _list.Count; i++)
            {
                var iResult = _list[i].VisitType(context).MoveHandler(category);
                if(!iResult.IsEmpty)
                {
                    NotImplementedMethod(category, context, currentCompilePosition, "i", i, "iResult", iResult);
                }
            }
            return Reni.Type.Base.EmptyHandler(category);
        }

        internal StructContainerSearchResult Search(DefineableToken defineableToken)
        {
            var result = defineableToken.TokenClass.SearchFromStruct();
            if(result != null)
                return result; // new OperationResult(defineableToken);

            if(!Dictionary.ContainsKey(defineableToken.Name))
                return null;

            return new StructAccess(Dictionary[defineableToken.Name]);
        }

        public Result VisitOperationApply(Reni.Context.Base definingParentContext, Reni.Context.Base callContext,
            Category category, Syntax.Base args)
        {
            var indexValue = args.VisitAndEvaluate(callContext, IndexType);
            var index = indexValue.ToInt32();
            return VisitElementFromContextRef(definingParentContext, category, index);
        }

        /// <summary>
        /// Visits the apply.
        /// </summary>
        /// <param name="structContext">The struct context.</param>
        /// <param name="position">The position.</param>
        /// <param name="callContext">The call context.</param>
        /// <param name="category">The category.</param>
        /// <param name="args">The args.</param>
        /// <returns></returns>
        /// created 13.12.2006 00:43
        internal Result VisitAccessApply(Reni.Context.Base structContext, int position, Reni.Context.Base callContext,
            Category category,
            Syntax.Base args)
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
    }
}