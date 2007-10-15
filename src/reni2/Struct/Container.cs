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
using Base=Reni.Syntax.Base;
using Void=Reni.Type.Void;

namespace Reni.Struct
{
    /// <summary>
    /// Structured data, context free version
    /// </summary>
    public class Container : ReniObject
    {
        private readonly List<Base> _list;
        private readonly List<Base> _converterList;

        private DictionaryEx<int, string> _rdict;

        private readonly DictionaryEx<string, int> _dictionary;

        public static bool _isInDump = false;
        private static int _nextObjectId = 0;
        private static readonly string _runId = Compiler.FormattedNow + "\n";
        private static bool _isInsideFileDump = false;

        [Node]
        public List<Base> List { get { return _list; } }

        [Node]
        public List<Base> ConverterList { get { return _converterList; } }

        [Node, DumpData(false)]
        public DictionaryEx<int, string> Rdict
        {
            get
            {
                if (_rdict == null)
                    CreateRDict();
                return _rdict;
            }
        }

        [Node]
        public DictionaryEx<string, int> Dictionary { get { return _dictionary; } }

        private Container
            (
            List<Base> list,
            List<Base> converterList,
            DictionaryEx<string, int> dict
            )
            : base(_nextObjectId++)
        {
            _list = list;
            foreach (Base elem in _list)
                Tracer.Assert(!(elem is Syntax.Struct));

            _converterList = converterList;
            _dictionary = dict;
        }

        private void CreateRDict()
        {
            _rdict = new DictionaryEx<int, string>();
            foreach (KeyValuePair<string, int> pair in _dictionary)
                _rdict[pair.Value] = pair.Key;
        }

        private Container(List<Base> list)
            : this(list, new List<Base>(), new DictionaryEx<string, int>())
        {
        }

        private Container(List<Base> list, DictionaryEx<string, int> dictionary)
            : this(list, new List<Base>(), dictionary)
        {
        }

        internal static Container Create(Base left)
        {
            List<Base> list = new List<Base>();
            list.Add(left);
            return new Container(list);
        }

        internal static Container Create(Base left, Base right)
        {
            List<Base> list = new List<Base>();
            list.Add(left);
            list.Add(right);
            return new Container(list);
        }

        internal static Container Create(DeclarationSyntax left, Base right)
        {
            List<Base> list = new List<Base>();
            list.Add(left.Definition);
            list.Add(right);
            DictionaryEx<string, int> dictionary = new DictionaryEx<string, int>();
            dictionary[left.DefineableToken.Name] = 0;
            return new Container(list, dictionary);
        }

        internal static Container Create(DeclarationSyntax left)
        {
            List<Base> list = new List<Base>();
            list.Add(left.Definition);
            DictionaryEx<string, int> dictionary = new DictionaryEx<string, int>();
            dictionary[left.DefineableToken.Name] = 0;
            return new Container(list, dictionary);
        }

        internal static Container Create(ConverterSyntax left)
        {
            List<Base> list = new List<Base>();
            List<Base> converter = new List<Base>();
            converter.Add(left.Body);
            DictionaryEx<string, int> dictionary = new DictionaryEx<string, int>();
            return new Container(list, converter, dictionary);
        }

        internal static Container Create(Base left, Container right)
        {
            List<Base> list = new List<Base>();
            list.Add(left);
            list.AddRange(right._list);
            DictionaryEx<string, int> dictionary = new DictionaryEx<string, int>();
            foreach (KeyValuePair<string, int> pair in right._dictionary)
                dictionary[pair.Key] = pair.Value + 1;

            return new Container(list, right._converterList, dictionary);
        }

        internal static Container Create(DeclarationSyntax left, Container right)
        {
            List<Base> list = new List<Base>();
            list.Add(left.Definition);
            list.AddRange(right._list);
            DictionaryEx<string, int> dictionary = new DictionaryEx<string, int>();
            dictionary[left.DefineableToken.Name] = 0;
            foreach (KeyValuePair<string, int> pair in right._dictionary)
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
            string result = "";
            for (int i = 0; i < _list.Count; i++)
            {
                if (i > 0)
                    result += ";";
                if (_rdict.ContainsKey(i))
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
            bool isInsideFileDump = _isInsideFileDump;
            _isInsideFileDump = true;
            string result = isInsideFileDump ? DumpDataToString() : DumpDataToFile();
            _isInsideFileDump = isInsideFileDump;
            return result;
        }

        private string DumpDataToFile()
        {
            File dumpFile = File.m("struct." + ObjectId);
            string oldResult = dumpFile.String;
            string newResult = _runId + DumpDataToString();
            if (oldResult == null || !oldResult.StartsWith(_runId))
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
            bool isInDump = _isInDump;
            _isInDump = true;
            string result = base.DumpData();
            _isInDump = isInDump;
            return result;
        }

        /// <summary>
        /// Visitor function
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="category">The category.</param>
        /// <param name="fromPosition">From position.</param>
        /// <param name="fromNotPosition">From not position.</param>
        /// <returns></returns>
        public Result Visit(Reni.Context.Base context, Category category, int fromPosition, int fromNotPosition)
        {
            bool trace =
                ObjectId == 109 &&
                context.ObjectId == 5 &&
                category.ToString() == "Size,Type,Refs,Code";
            //trace = false;
            StartMethodDumpWithBreak(trace, context, category, fromPosition, fromNotPosition);
            Result result = Reni.Type.Base.CreateVoidResult(category - Category.Type);
            Code.Base topRef = context.CreateTopRefCode();
            for (int i = fromPosition; i < fromNotPosition; i++)
            {
                Reni.Context.Base structContext = context.CreateStruct(this, i);
                Result iresult = _list[i].Visit(structContext, category).Align(context.RefAlignParam.AlignBits);
                if (iresult.IsPending)
                    return ReturnMethodDump(trace, iresult);
                if (trace) DumpDataWithBreak("", "i", i, "result", result, "iresult", iresult);
                result.Add(iresult);
            }
            if (category.HasType)
                result.Type = CreateStructType(context, fromNotPosition);
            Result resultReplaced = result.ReplaceRelativeContextRef(context.CreateStructContainer(this), topRef);
            return ReturnMethodDump(trace, resultReplaced);
        }

        private Reni.Type.Base CreateStructType(Reni.Context.Base context, int currentCompilePosition)
        {
            return context.CreateStructContainer(this).CreateStructType(currentCompilePosition);
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
                .CreateStruct(this, position)
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
        public Result DestructorHandler(Reni.Context.Base context, Category category, Result objRefResult, int accessPosition)
        {
            Result result = Reni.Type.Base.CreateVoidResult(category - Category.Type - Category.Size);
            Size size = VisitSize(context, 0, accessPosition);
            for (int i = 0; i < accessPosition; i++)
            {
                Reni.Type.Base iType = VisitElementTypeFromContextRef(context, i);
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
        /// <param name="fromPosition">From position.</param>
        /// <param name="fromNotPosition">From not position.</param>
        /// <returns></returns>
        /// created 09.12.2006 00:33
        public Size VisitSize(Reni.Context.Base context, int fromPosition, int fromNotPosition)
        {
            return Visit(context, Category.Size, fromPosition, fromNotPosition).Size;
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
            return Visit(context, Category.Type, 0, currentCompilePosition).Type;
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
            return Visit(context, category, 0, _list.Count);
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
        /// indexer
        /// </summary>
        /// <value></value>
        /// created 15.12.2006 23:55
        public Base this[int index] { get { return _list[index]; } }

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
            List<Result> result = DumpPrintFromRef(category, context);
            return ConcatPrintResult(category, result);
        }

        private static Result ConcatPrintResult(Category category, IList<Result> elemResults)
        {
            Result result = Void.CreateResult(category);
            if (category.HasCode)
                result.Code = Code.Base.CreateDumpPrintText("(");

            for (int i = 0; i < elemResults.Count; i++)
            {
                if (category.HasCode)
                {
                    if (i > 0)
                        result.Code = result.Code.CreateSequence(Code.Base.CreateDumpPrintText(", "));
                    result.Code = result.Code.CreateSequence(elemResults[i].Code);
                }
                if (category.HasRefs)
                    result.Refs = result.Refs.Pair(elemResults[i].Refs);
            }
            if (category.HasCode)
                result.Code = result.Code.CreateSequence(Code.Base.CreateDumpPrintText(")"));
            return result;
        }

        private List<Result> DumpPrintFromRef(Category category, Reni.Context.Base context)
        {
            List<Result> result = new List<Result>();
            ContainerContext containerContext = context.CreateStructContainer(this);
            for (int i = 0; i < _list.Count; i++)
            {
                Result iResult = VisitElementTypeFromContextRef(context, i).DumpPrint(category);
                Result iAccess = containerContext.CreateStructType(_list.Count).AccessFromArg(category, i);
                iResult = iResult.UseWithArg(iAccess);
                result.Add(iResult);
            }
            return result;
        }

        public bool HasConverterTo(Reni.Context.Base context, Reni.Type.Base dest)
        {
            for (int i = 0; i < _converterList.Count; i++)
            {
                if (
                    _converterList[i].VisitType(context).IsConvertableTo(dest,
                                                                         ConversionFeature.Instance.DontUseConverter()))
                {
                    for (i++; i < _converterList.Count; i++)
                        Tracer.Assert(
                            !_converterList[i].VisitType(context).IsConvertableTo(dest,
                                                                                  ConversionFeature.Instance.
                                                                                      DontUseConverter()));
                    return true;
                }
            }
            return false;
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
            for (int i = 0; i < _list.Count; i++)
            {
                if (!_list[i].VisitType(context).IsVoid)
                    return false;
            }
            return true;
        }

        public bool IsPendingType(Reni.Context.Base context)
        {
            for (int i = 0; i < _list.Count; i++)
            {
                Reni.Context.Base structContext = context.CreateStruct(this, i);
                if (_list[i].VisitType(structContext).IsPending)
                    return true;
            }
            return false;
        }

        internal Result ConvertTo(Category category, Reni.Context.Base context, Reni.Type.Base dest)
        {
            // Special case if dest is Void and size is zero
            if (dest is Void && VisitSize(context, 0, _list.Count).IsZero)
                return Reni.Type.Base.CreateVoid.CreateArgResult(category);

            List<Reni.Type.Base> converterTypes = context.CreateStruct(this).VisitType(_converterList);


            NotImplementedMethod(category, context, dest, "convertTypes", converterTypes);
            return null;
        }

        /// <summary>
        /// Offsets the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        internal Size BackOffset(Reni.Context.Base context, int index)
        {
            return VisitSize(context, 0, index + 1)*-1;
        }

        /// <summary>
        /// Offsets the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        internal Size Offset(Reni.Context.Base context, int index)
        {
            return VisitSize(context, index, _list.Count);
        }


        [DumpData(false)]
        internal Reni.Type.Base IndexType
        {
            get
            {
                return MaxIndexType;
                //return Type.Base.CreateNumber(BitsConst.Convert(_list.Count).Size.ToInt());
            }
        }

        internal static Reni.Type.Base MaxIndexType { get { return Reni.Type.Base.CreateNumber(32); } }

        internal Code.Base CreateRef(Reni.Context.Base parent)
        {
            return parent.CreateTopRefCode().CreateRefPlus(parent.RefAlignParam, Offset(parent, 0));
        }

        public Result MoveHandler(Category category, Reni.Context.Base context, int currentCompilePosition)
        {
            for (int i = 0; i < _list.Count; i++)
            {
                Result iResult = _list[i].VisitType(context).MoveHandler(category);
                if (!iResult.IsEmpty)
                {
                    NotImplementedMethod(category, context, currentCompilePosition, "i", i, "iResult", iResult);
                }
            }
            return Reni.Type.Base.EmptyHandler(category);
        }

        internal StructContainerSearchResult SearchDefineable(DefineableToken defineableToken)
        {
            if (defineableToken.TokenClass.IsStructOperation)
                return new OperationResult(defineableToken);

            if (!Dictionary.ContainsKey(defineableToken.Name))
                return null;

            int resultPosition = Dictionary[defineableToken.Name];
            return new StructAccess(defineableToken, resultPosition);
        }

        public Result VisitOperationApply(Reni.Context.Base definingParentContext, Reni.Context.Base callContext, Category category, Base args)
        {
            BitsConst indexValue = args.VisitAndEvaluate(callContext, IndexType);
            int index = indexValue.ToInt32();
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
        internal Result VisitAccessApply(Reni.Context.Base structContext, int position, Reni.Context.Base callContext, Category category, 
            Base args)
        {
            Category localCategory = category;
            if (args != null)
                localCategory = localCategory | Category.Type;
            Result functionResult = VisitElementFromContextRef(structContext, localCategory, position);
            if (args == null)
                return functionResult;

            if (functionResult.IsCodeLess)
                return functionResult.Type.ApplyFunction(callContext, category, args);

            NotImplementedMethod(position, callContext, category, args);
            return null;
        }

    }

}