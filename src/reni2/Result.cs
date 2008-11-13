using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Code;
using Reni.Context;
using Reni.Syntax;
using Reni.Type;

namespace Reni
{
    /// <summary>
    /// Result of a visitor request
    /// </summary>
    [Serializable]
    internal sealed class Result : ReniObject, ITreeNodeSupport, Sequence<Result>.ICombiner<Result>
    {
        private bool _isDirty;
        private Category _pending;
        private Size _size;
        private TypeBase _type;
        private CodeBase _code;
        private Refs _refs;
        internal PostProcessorForResult PostProcessor;

        /// <summary>
        /// ctor
        /// </summary>
        public Result()
        {
            PostProcessor = new PostProcessorForResult(this);
            _pending = new Category();
        }

        private bool HasSize { get { return Size != null; } }
        internal bool HasType { get { return Type != null; } }
        internal bool HasCode { get { return Code != null; } }
        internal bool HasRefs { get { return Refs != null; } }

        /// <summary>
        /// In case of recursion, there may be pending requests
        /// </summary>
        [Node]
        private Category Pending { get { return _pending; } set { _pending = value; } }

        /// <summary>
        /// Obtain the category signature contained
        /// </summary>
        public Category Complete { get { return new Category(HasSize, HasType, HasCode, HasRefs, HasInternal); } }

        [Node]
        public Size Size
        {
            get { return _size; }
            set
            {
                _size = value;
                AssertValid();
            }
        }

        [Node]
        public TypeBase Type
        {
            get { return _type; }
            set
            {
                _type = value;
                AssertValid();
            }
        }

        [Node]
        internal CodeBase Code
        {
            get { return _code; }
            set
            {
                _code = value;
                AssertValid();
            }
        }

        [Node]
        public Refs Refs
        {
            get { return _refs; }
            set
            {
                _refs = value;
                AssertValid();
            }
        }

        [Node]
        public Sequence<IInternalResultProvider> Internal
        {
            get { return _internal; }
            set
            {
                _internal = value;
                AssertValid();
            }
        }

        TreeNode[] ITreeNodeSupport.CreateNodes()
        {
            var result = new List<TreeNode>();
            if(!Pending.IsNull)
                result.Add(Service.CreateNamedNode("Pending", "Pending", Dump()));
            if(HasSize)
                result.Add(Service.CreateNamedNode("Size", "Number", Size.FormatForView()));
            if(HasType)
                result.Add(Service.CreateNamedNode("Type", "Type", Type));
            if(HasCode)
                result.Add(Service.CreateNamedNode("Code", "Code", Code));
            if(HasRefs)
                result.Add(Service.CreateNamedNode("Refs", "Refs", Refs.Data));
            if(HasInternal)
                result.Add(Service.CreateNamedNode("Internal", "Code", Internal));
            return result.ToArray();
        }

        /// <summary>
        /// Returns the size by checking category size, type and code until a result can be found. 
        /// Otherwise null is returned
        /// </summary>
        public Size FindSize
        {
            get
            {
                if(HasSize)
                    return Size;
                if(HasType)
                    return Type.Size;
                if(HasCode)
                    return Code.Size;
                return null;
            }
        }

        /// <summary>
        /// Returns the size by checking category size, type and code until a result can be found. 
        /// Otherwise an exception is thrown
        /// </summary>
        internal Size SmartSize
        {
            get
            {
                var result = FindSize;
                if(result == null)
                {
                    DumpMethodWithBreak("No approriate result property defined");
                    Debugger.Break();
                }
                return result;
            }
        }

        /// <summary>
        /// Error handling
        /// </summary>
        internal static Error Error { get { return null; } }

        internal bool IsDirty
        {
            get { return _isDirty; }
            set
            {
                _isDirty = value;
                AssertValid();
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is empty.
        /// </summary>
        /// <value><c>true</c> if this instance is empty; otherwise, <c>false</c>.</value>
        /// [created 05.06.2006 18:06]
        internal bool IsEmpty
        {
            get
            {
                if(Complete.HasSize && !Size.IsZero)
                    return false;
                if(Complete.HasType && !Type.IsVoid)
                    return false;
                if(Complete.HasCode && !Code.IsEmpty)
                    return false;
                if(Complete.HasRefs && !Refs.IsNone)
                    return false;
                return true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is has no code to execute.
        /// </summary>
        /// <value><c>true</c> if this instance is empty; otherwise, <c>false</c>.</value>
        /// [created 05.06.2006 18:06]
        internal bool IsCodeLess
        {
            get
            {
                if(Complete.HasSize && !Size.IsZero)
                    return false;
                if(Complete.HasCode && !Code.IsEmpty)
                    return false;
                if(Complete.HasRefs && !Refs.IsNone)
                    return false;
                return true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is pending.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is pending; otherwise, <c>false</c>.
        /// </value>
        internal bool IsPending
        {
            get
            {
                if(Complete.HasSize)
                    return Size.IsPending;
                if(Complete.HasType)
                    return Type.IsPending;
                if(Complete.HasCode)
                    return Code.IsPending;
                if(Complete.HasRefs)
                    return Refs.IsPending;

                return false;
            }
        }

        /// <summary>
        /// asis
        /// </summary>
        /// <returns></returns>
        public override string DumpData()
        {
            var result = "";
            result += "Pending=" + _pending.Dump();
            result += "\n";
            result += "Complete=" + Complete.Dump();
            if(HasSize)
                result += "\nSize=" + Tracer.Dump(_size);
            if(HasType)
                result += "\nType=" + Tracer.Dump(_type);
            if(HasRefs)
                result += "\nRefs=" + Tracer.Dump(_refs);
            if(HasCode)
                result += "\nCode=" + Tracer.Dump(_code);
            if(HasInternal)
                result += "\nInternal=" + Tracer.Dump(_internal);
            return result;
        }

        /// <summary>
        /// Adds categories to pending requests
        /// </summary>
        /// <param name="frEff"></param>
        private void AddPending(Category frEff) { _pending |= frEff; }

        /// <summary>
        /// Add categories
        /// </summary>
        /// <param name="r"></param>
        internal void Update(Result r)
        {
            if(r.HasSize && !r.Size.IsPending)
                _size = r.Size;

            if(r.HasType && !r.Type.IsPending)
                _type = r.Type;

            if(r.HasRefs && !r.Refs.IsPending)
                _refs = r.Refs;

            if(r.HasCode && !r.Code.IsPending)
                _code = r.Code;

            if(r.HasInternal)
                _internal = r.Internal;

            AssertValid();
        }

        private void Update(Result r, Category c)
        {
            if(c.HasSize)
                _size = r.Size ?? Size.Pending;
            if(c.HasType)
                _type = r.Type ?? TypeBase.Pending;
            if(c.HasRefs)
                _refs = r.Refs ?? Refs.Pending;
            if(c.HasCode)
                _code = r.Code ?? CodeBase.Pending;
            if(c.HasInternal)
            {
                Tracer.Assert(r.HasInternal);
                _internal = r.Internal;
            }
            AssertValid();
        }

        internal Result Filter(Category category)
        {
            if(category == Complete)
                return this;

            var result = new Result();
            result.Update(this, category);
            return result;
        }

        /// <summary>
        /// Aligns the result to number of bits provided
        /// </summary>
        /// <param name="alignBits">Bits to align result</param>
        /// <returns></returns>
        internal Result Align(int alignBits)
        {
            var size = FindSize;
            if(size == null || size.IsPending)
                return this;

            var alignedSize = size.Align(alignBits);
            if(alignedSize == size)
                return this;

            var r = new Result();
            if(HasSize)
                r.Size = alignedSize;
            if(HasType)
                r.Type = Type.CreateAlign(alignBits);
            if(HasCode)
                r.Code = Code.CreateBitCast(alignedSize);
            if(HasRefs)
                r.Refs = Refs;
            if(HasInternal)
                r.Internal = Internal;
            return r;
        }

        /// <summary>
        /// Create a flat copy
        /// </summary>
        internal Result Clone(Category category)
        {
            var r = new Result();
            if(category.HasSize)
                r.Size = Size;
            if(category.HasType)
                r.Type = Type;
            if(category.HasCode)
                r.Code = Code;
            if(category.HasRefs)
                r.Refs = Refs;
            if(category.HasInternal)
                r.Internal = Internal;
            return r;
        }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns></returns>
        /// created 19.11.2006 22:13
        internal Result Clone() { return Clone(Complete); }

        private void AssertValid()
        {
            if(IsDirty)
                return;

            var size = FindSize;
            if(size != null)
            {
                if(HasSize && !(Size == size))
                    Tracer.AssertionFailed(1, @"Size==size", "Size differs " + Dump());
                if(HasType && !(Type.Size == size))
                    Tracer.AssertionFailed(1, @"Type.Size==size", "Type size differs " + Dump());
                if(HasCode && !(Code.Size == size))
                    Tracer.AssertionFailed(1, @"Code.Size==size", "Code size differs " + Dump());
            }

            if(HasRefs && HasCode && !Refs.Contains(Code.GetRefs()))
                Tracer.AssertionFailed(1, @"Refs.Contains(codeRefs)", "Code and Refs differ " + Dump());
        }

        private bool HasCategory(Category category)
        {
            if(category.HasSize && !Complete.HasSize)
                return false;
            if(category.HasType && !Complete.HasType)
                return false;
            if(category.HasCode && !Complete.HasCode)
                return false;
            if(category.HasRefs && !Complete.HasRefs)
                return false;
            if(category.HasInternal && !Complete.HasInternal)
                return false;
            return true;
        }

        /// <summary>
        /// Visitor function
        /// </summary>
        /// <param name="category"></param>
        /// <param name="context"></param>
        /// <param name="syntax"></param>
        /// <returns></returns>
        [DebuggerHidden]
        internal Result AddCategories(Category category, ContextBase context, ICompileSyntax syntax)
        {
            var OldPending = Pending;
            var OldComplete = Complete;
            var notPendingCategory = category - OldPending;
            var notCompleteCategory = notPendingCategory - OldComplete;
            if(!notCompleteCategory.IsNull)
            {
                AddPending(notCompleteCategory);

                try
                {
                    var result = syntax.Result(context, notCompleteCategory);
                    context.AssertCorrectRefs(this);
                    result.AssertComplete(notCompleteCategory, syntax);
                    Update(result);
                }
                catch
                {
                    Tracer.FlaggedLine("CCompilerMsg(*mSyntaxKey->TokenClass, \"aftereffect\").Show()");
                    throw;
                }

                Pending = OldPending;
            }
            var filteredResult = Filter(category);
            Tracer.Assert(filteredResult.Complete == category, string.Format("syntax={2}\ncategory={0}\nResult={1}", category, filteredResult.Dump(), syntax.DumpShort()));
            return filteredResult;
        }

        internal void AssertComplete(Category category, ICompileSyntax syntaxForDump) { Tracer.Assert(1, HasCategory(category), string.Format("syntax={2}\ncategory={0}\nResult={1}", category, Dump(), syntaxForDump.DumpShort())); }

        internal void AssertComplete(Category category) { Tracer.Assert(1, HasCategory(category), string.Format("category={0}\nResult={1}", category, Dump())); }

        /// <summary>
        /// Append two results together, categories are determined by first 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        internal void Add(Result other) { Add(other, Complete); }

        internal void Add(Result other, Category category)
        {
            Tracer.Assert(other.HasCategory(category));
            Tracer.Assert(HasCategory(category));
            IsDirty = true;
            if(category.HasSize)
                Size += other.Size;
            if(category.HasType)
                Type = Type.CreatePair(other.Type);
            if(category.HasCode)
                Code = Code.CreateSequence(other.Code);
            if(category.HasRefs)
                Refs = Refs.CreateSequence(other.Refs);
            if(category.HasInternal)
                Internal = Internal + other.Internal;
            IsDirty = false;
        }

        /// <summary>
        /// Creates the sequence.
        /// </summary>
        /// <param name="second">The second.</param>
        /// <param name="category">The category.</param>
        /// <returns></returns>
        /// created 19.11.2006 21:50
        internal Result CreateSequence(Result second, Category category)
        {
            var result = Clone(category);
            result.Add(second);
            return result;
        }

        public Result CreateSequence(Result second)
        {
            var result = Clone();
            result.Add(second);
            return result;
        }

        /// <summary>
        /// Append two results together, categories are determined by first, replaces arg 
        /// </summary>
        /// <param name="result"></param>
        /// <param name="topRef"></param>
        /// <returns></returns>
        internal void AddStruct(Result result, Result topRef) { Add(result.UseWithArg(topRef)); }

        /// <summary>
        /// Converts the bit array to bit array.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="sourceBitCount">The source count.</param>
        /// <param name="destBitCount">The dest count.</param>
        /// <returns></returns>
        /// [created 30.05.2006 00:17]
        internal static Result ConvertBitArrayToBitArray(Category category, int sourceBitCount, int destBitCount)
        {
            if(sourceBitCount == 0 && destBitCount == 0)
                return TypeBase.CreateVoidResult(category);
            var result = new Result();
            if(category.HasCode)
            {
                var codeResult = CodeBase.CreateArg(Size.Create(sourceBitCount));
                if(destBitCount != sourceBitCount)
                    codeResult = codeResult.CreateBitCast(Size.Create(destBitCount));
                result.Code = codeResult;
            }
            if(category.HasRefs)
                result.Refs = Refs.None();
            return result;
        }

        /// <summary>
        /// Uses the with arg.
        /// </summary>
        /// <param name="resultForArg">The result to use as replacment.</param>
        /// <returns></returns>
        /// [created 30.05.2006 00:25]
        internal Result UseWithArg(Result resultForArg)
        {
            if(IsPending)
                return this;

            var trace = ObjectId == 1490 && resultForArg.ObjectId == 1499;
            StartMethodDump(trace, resultForArg);
            var result = new Result {Size = Size, Type = Type, Internal = Internal};
            if(HasCode && resultForArg.HasCode)
                result.Code = Code.UseWithArg(resultForArg.Code);
            if(HasRefs && resultForArg.HasRefs)
                result.Refs = Refs.CreateSequence(resultForArg.Refs);
            if(HasInternal && resultForArg.HasInternal)
                result.Internal = Internal +resultForArg.Internal;
            return ReturnMethodDump(trace, result);
        }

        /// <summary>
        /// Replaces the context ref.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="replacement">The replacement.</param>
        /// <returns></returns>
        internal Result ReplaceAbsoluteContextRef<C>(C context, Result replacement) where C : IContextRefInCode
        {
            if(IsPending)
                return this;

            if(HasRefs && !Refs.Contains(context))
                return this;

            var result = new Result {Size = Size, Type = Type, Internal = Internal, IsDirty = true};
            if(HasCode)
                result.Code = Code.ReplaceAbsoluteContextRef(context, replacement.Code);
            if(HasRefs)
                result.Refs = Refs.Without(context).CreateSequence(replacement.Refs);
            result.IsDirty = false;
            return result;
        }

        /// <summary>
        /// Replaces the context ref.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="replacement">The replacement.</param>
        /// <returns></returns>
        internal Result ReplaceRelativeContextRef<C>(C context, CodeBase replacement) where C : IContextRefInCode
        {
            if(HasRefs && !Refs.Contains(context))
                return this;

            var result = new Result {Size = Size, Type = Type, Internal = Internal};
            if(HasCode)
                result.Code = Code.ReplaceRelativeContextRef(context, replacement);
            if(HasRefs)
                result.Refs = Refs.Without(context);
            return result;
        }

        /// <summary>
        /// Replaces the refs.
        /// </summary>
        /// <param name="refAlignParam">The ref align param.</param>
        /// <param name="replacement">The replacement.</param>
        /// <returns></returns>
        /// created 31.12.2006 14:50
        internal Result ReplaceRefsForFunctionBody(RefAlignParam refAlignParam, CodeBase replacement)
        {
            if(!HasCode)
                return this;
            if(Refs.Count == 0)
                return this;
            var result = Clone();
            result.IsDirty = true;
            result.Code = Refs.ReplaceRefsForFunctionBody(Code, refAlignParam, replacement);
            result.Refs = Refs.None();
            result.IsDirty = false;
            return result;
        }

        /// <summary>
        /// Combines two results .
        /// </summary>
        /// <param name="result">The result.</param>
        /// <param name="category">The category.</param>
        /// <returns>Pair of this and other. If type has </returns>
        /// created 19.11.2006 22:04
        internal Result SafeList(Result result, Category category)
        {
            var destructorResult = Type.DestructorHandler(category);
            if(destructorResult.IsEmpty)
                return CreateSequence(result, category);

            NotImplementedMethod(result, category);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates the plus op.
        /// </summary>
        /// <param name="c">The c.</param>
        /// <param name="refAlignParam">The ref align param.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        /// [created 04.06.2006 01:09]
        internal Result CreateRefPlus(Category c, RefAlignParam refAlignParam, Size value)
        {
            var result = Clone();
            if(c.HasCode)
                result.Code = Code.CreateRefPlus(refAlignParam, value);
            return result;
        }

        internal Result CreateStatement(Category category)
        {
            if (!HasInternal)
                return Filter(category);
            if (Internal.IsEmpty)
                return this;

            NotImplementedMethod(category);
            return null;
            /*
            var internalResults = CollectInternalResults(category);
            var internalResult = internalResults.Serialize(TypeBase.CreateVoid.CreateResult(category));

            var destructorResults = internalResults.Apply1(x => x.Type.DestructorHandler(category));
            var result = Clone(category - Category.Internal);
            result.Internal = EmptyInternal;
            var moveResult = Type.MoveHandler(category);

            if(category.HasRefs)
                result.Refs = internalResult.Refs
                    .CreateSequence(result.Refs)
                    .CreateSequence(destructorResults.Apply(x=>x.Refs).Serialize(Refs.None()))
                    .CreateSequence(moveResult.Refs);
            if(category.HasCode)
            {
                var resultCode = internalResult.Code.CreateStatementEndFromIntermediateStorage
                    (
                    Code,
                    destructorResults.Apply(x=>x.Code).Serialize(CodeBase.CreateVoid()),
                    moveResult.Code
                    );
                result.Code = resultCode;
            }
            result.AssertValid();
            return result;
             */
        }

        private Sequence<Result> CollectInternalResults(Category category)
        {
            return Internal.Apply(x =>
            {
                var last = x.Result(category | Category.Type | Category.Internal);
                return last.CollectInternalResults(category) + last;
            });
        }

        internal static Result CreatePending(Category category)
        {
            var result = new Result();
            if(category.HasSize)
                result.Size = Size.Pending;
            if(category.HasType)
                result.Type = TypeBase.Pending;
            if(category.HasRefs)
                result.Refs = Refs.Pending;
            if(category.HasCode)
                result.Code = CodeBase.Pending;
            return result;
        }

        internal Result ConvertTo(TypeBase target) { return Type.Conversion(Complete, target).UseWithArg(this); }

        internal Result CreateUnref(TypeBase type, RefAlignParam refAlignParam)
        {
            return type.CreateResult
                (
                Complete,
                () => Code.CreateDereference(refAlignParam, type.Size),
                () => Refs
                );
        }

        internal Result DumpPrintBitSequence()
        {
            return Reni.Type.Void.CreateResult
                (
                Complete,
                () => Code.CreateDumpPrint(),
                () => Refs
                );
        }

        internal BitsConst Evaluate()
        {
            Tracer.Assert(Refs.IsNone);
            return Code.Serialize(false).Evaluate();
        }

        internal Result UnProperty() { return Type.UnProperty(this); }

        internal Result AutomaticDereference()
        {
            if(IsPending || Complete == Category.Refs)
                return this;

            return Type.AutomaticDereference(this);
        }

        internal Result PostProcess(int alignBits) { return PostProcess().Align(alignBits); }

        internal Result PostProcess() { return UnProperty().AutomaticDereference(); }

        internal static Result ConcatPrintResult(Category category, IList<Result> elemResults)
        {
            var result = Reni.Type.Void.CreateResult(category);
            if(category.HasCode)
                result.Code = CodeBase.CreateDumpPrintText("(");

            for(var i = 0; i < elemResults.Count; i++)
            {
                if(category.HasCode)
                {
                    if(i > 0)
                        result.Code = result.Code.CreateSequence(CodeBase.CreateDumpPrintText(", "));
                    result.Code = result.Code.CreateSequence(elemResults[i].Code);
                }
                if(category.HasRefs)
                    result.Refs = result.Refs.CreateSequence(elemResults[i].Refs);
            }
            if(category.HasCode)
                result.Code = result.Code.CreateSequence(CodeBase.CreateDumpPrintText(")"));
            return result;
        }

        internal static Sequence<IInternalResultProvider> EmptyInternal = HWString.Sequence<IInternalResultProvider>() ;
    }

    internal interface IInternalResultProvider : IResultProvider
    {
    }

    /// <summary>
    /// Describes errors, not yet implemented
    /// </summary>
    internal sealed class Error
    {
        private readonly ContextBase _context;
        private readonly ICompileSyntax _syntax;

        /// <summary>
        /// Initializes a new instance of the <see cref="Error"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="syntax">The syntax.</param>
        /// created 29.10.2006 18:23
        internal Error(ContextBase context, ICompileSyntax syntax)
        {
            _context = context;
            _syntax = syntax;
        }

        private Error(Error e0, Error e1) { }

        /// <summary>
        /// asis
        /// </summary>
        /// <param name="e0"></param>
        /// <param name="e1"></param>
        /// <returns></returns>
        public static Error operator +(Error e0, Error e1)
        {
            if(e0 == null)
                return e1;
            return e1 == null ? e0 : new Error(e0, e1);
        }

        internal ContextBase Context { get { return _context; } }
        internal ICompileSyntax Syntax { get { return _syntax; } }
    }
}
