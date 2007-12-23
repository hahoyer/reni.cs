using System;
using System.Collections.Generic;
using System.Diagnostics;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper.TreeViewSupport;
using Reni.Context;
using Base=Reni.Code.Base;
using Void=Reni.Type.Void;

namespace Reni
{
    /// <summary>
    /// Result of a visitor request
    /// </summary>
    internal sealed class Result : ReniObject
    {
        ///<summary>
        /// Delegate that returns code
        ///</summary>
        internal delegate Base GetCode();

        ///<summary>
        /// Delegate that returns refs
        ///</summary>
        internal delegate Refs GetRefs();

        private Category _pending;
        private Size _size;
        private Type.Base _type;
        private Base _code;
        private Refs _refs;
        private bool _isDirty = false;

        /// <summary>
        /// asis
        /// </summary>
        /// <returns></returns>
        public override string DumpData()
        {
            string result = "";
            result += "Pending=" + _pending.Dump();
            result += "\n";
            result += "Complete=" + Complete.Dump();
            if (HasSize) result += "\nSize=" + Tracer.Dump(_size);
            if (HasType) result += "\nType=" + Tracer.Dump(_type);
            if (HasRefs) result += "\nRefs=" + Tracer.Dump(_refs);
            if (HasCode) result += "\nCode=" + Tracer.Dump(_code);
            return result;
        }

        /// <summary>
        /// ctor
        /// </summary>
        public Result()
        {
            _pending = new Category();
        }

        private bool HasSize { get { return Size != null; } }
        private bool HasType { get { return Type != null; } }
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
        public Category Complete { get { return new Category(HasSize, HasType, HasCode, HasRefs); } }

        /// <summary>
        /// The size-category, can be null
        /// </summary>
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

        /// <summary>
        /// The type-category, can be null
        /// </summary>
        [Node]
        public Type.Base Type
        {
            get { return _type; }
            set
            {
                _type = value;
                AssertValid();
            }
        }

        /// <summary>
        /// The code-category, can be null
        /// </summary>
        [Node]
        internal Base Code
        {
            get { return _code; }
            set
            {
                _code = value;
                AssertValid();
            }
        }

        /// <summary>
        /// The refs-category, can be null
        /// </summary>
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

        /// <summary>
        /// Adds categories to pending requests
        /// </summary>
        /// <param name="frEff"></param>
        private void AddPending(Category frEff)
        {
            _pending |= frEff;
        }

        /// <summary>
        /// Add categories
        /// </summary>
        /// <param name="r"></param>
        private void Update(Result r)
        {
            if (r.HasSize && !r.Size.IsPending)
                _size = r.Size;

            if (r.HasType && !r.Type.IsPending)
                _type = r.Type;

            if (r.HasRefs && !r.Refs.IsPending)
                _refs = r.Refs;

            if (r.HasCode && !r.Code.IsPending)
                _code = r.Code;
        }

        private void Update(Result r, Category c)
        {
            if (c.HasSize)
            {
                _size = r.Size;
                if (_size == null)
                    _size = Size.Pending;
            }
            if (c.HasType)
            {
                _type = r.Type;
                if (_type == null)
                    _type = Reni.Type.Base.Pending;
            }
            if (c.HasRefs)
            {
                _refs = r.Refs;
                if (_refs == null)
                    _refs = Refs.Pending;
            }
            if (c.HasCode)
            {
                _code = r.Code;
                if (_code == null)
                    _code = Base.Pending;
            }
        }

        /// <summary>
        /// Returns the size by checking category size, type and code until a result can be found. 
        /// Otherwise null is returned
        /// </summary>
        public Size FindSize
        {
            get
            {
                if (HasSize) return Size;
                if (HasType) return Type.Size;
                if (HasCode) return Code.Size;
                return null;
            }
        }

        /// <summary>
        /// Returns the size by checking category size, type and code until a result can be found. 
        /// Otherwise an exception is thrown
        /// </summary>
        public Size SmartSize
        {
            get
            {
                Size result = FindSize;
                if (result == null)
                {
                    DumpMethodWithBreak("No approriate result property defined");
                    Debugger.Break();
                }
                return result;
            }
        }

        private Result Filter(Category category)
        {
            if (category == Complete)
                return this;

            Result result = new Result();
            result.Update(this, category);
            return result;
        }

        /// <summary>
        /// Aligns the result to number of bits provided
        /// </summary>
        /// <param name="alignBits">Bits to align result</param>
        /// <returns></returns>
        public Result Align(int alignBits)
        {
            Size size = FindSize;
            if (size == null)
            {
                Result r = new Result();
                if (HasRefs) r.Refs = Refs;
                return r;
            }

            if (size.IsPending)
                return this;

            Size alignedSize = size.Align(alignBits);
            if (alignedSize != size)
            {
                Result r = new Result();
                if (HasSize) r.Size = alignedSize;
                if (HasType) r.Type = Type.CreateAlign(alignBits);
                if (HasCode) r.Code = Code.CreateBitCast(alignedSize);
                if (HasRefs) r.Refs = Refs;
                return r;
            }
            return this;
        }

        /// <summary>
        /// Create a flat copy
        /// </summary>
        public Result Clone(Category category)
        {
            Result r = new Result();
            if (category.HasSize) r.Size = Size;
            if (category.HasType) r.Type = Type;
            if (category.HasCode) r.Code = Code;
            if (category.HasRefs) r.Refs = Refs;
            return r;
        }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns></returns>
        /// created 19.11.2006 22:13
        public Result Clone()
        {
            return Clone(Complete);
        }

        /// <summary>
        /// Error handling
        /// </summary>
        public Error Error { get { return null; } set { } }

        private void AssertValid()
        {
            if (IsDirty)
                return;

            Size size = FindSize;
            if (size != null)
            {
                if (HasSize)
                {
                    if (!(Size == size))
                    {
                        Tracer.AssertionFailed(1, @"Size==size", "Size differs " + Dump());
                        Debugger.Break();
                    }
                }
                if (HasType)
                {
                    if (!(Type.Size == size))
                    {
                        Tracer.AssertionFailed(1, @"Type.Size==size", "Type size differs " + Dump());
                        Debugger.Break();
                    }
                }
                ;
                if (HasCode)
                {
                    if (!(Code.Size == size))
                    {
                        Tracer.AssertionFailed(1, @"Code.Size==size", "Code size differs " + Dump());
                        Debugger.Break();
                    }
                }
                ;
            }

            if (HasRefs && HasCode)
            {
                if (!Refs.Contains(Code.Refs))
                {
                    Tracer.AssertionFailed(1, @"Refs.Contains(codeRefs)", "Code and Refs differ " + Dump());
                    Debugger.Break();
                }
            }
        }

        public bool IsDirty
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
        public bool IsEmpty
        {
            get
            {
                if (Complete.HasSize && !Size.IsZero) return false;
                if (Complete.HasType && !Type.IsVoid) return false;
                if (Complete.HasCode && !Code.IsEmpty) return false;
                if (Complete.HasRefs && !Refs.IsNone) return false;
                return true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is has no code to execute.
        /// </summary>
        /// <value><c>true</c> if this instance is empty; otherwise, <c>false</c>.</value>
        /// [created 05.06.2006 18:06]
        public bool IsCodeLess
        {
            get
            {
                if (Complete.HasSize && !Size.IsZero) return false;
                if (Complete.HasCode && !Code.IsEmpty) return false;
                if (Complete.HasRefs && !Refs.IsNone) return false;
                return true;
            }
        }

        private bool HasCategory(Category category)
        {
            if (category.HasSize && !Complete.HasSize) return false;
            if (category.HasType && !Complete.HasType) return false;
            if (category.HasCode && !Complete.HasCode) return false;
            if (category.HasRefs && !Complete.HasRefs) return false;
            return true;
        }
        /// <summary>
        /// Gets a value indicating whether this instance is pending.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is pending; otherwise, <c>false</c>.
        /// </value>
        public bool IsPending
        {
            get
            {
                if (Complete.HasSize) return Size.IsPending;
                if (Complete.HasType) return Type.IsPending;
                if (Complete.HasCode) return Code.IsPending;
                if (Complete.HasRefs) return Refs.IsPending;
                return false;
            }
        }

        /// <summary>
        /// Visitor function
        /// </summary>
        /// <param name="category"></param>
        /// <param name="context"></param>
        /// <param name="syntax"></param>
        /// <returns></returns>
        [DebuggerHidden]
        internal Result Visit(Category category, Context.Base context, Syntax.Base syntax)
        {
            Category OldPending = Pending;
            Category OldComplete = Complete;
            Category notPendingCategory = category - OldPending;
            Category notCompleteCategory = notPendingCategory - OldComplete;
            if (!notCompleteCategory.IsNull)
            {
                AddPending(notCompleteCategory);

                try
                {
                    Result RSub = syntax.VirtVisit(context, notCompleteCategory);
                    Update(RSub);
                }
                catch
                {
                    Tracer.FlaggedLine("CCompilerMsg(*mSyntaxKey->TokenClass, \"aftereffect\").Show()");
                    throw;
                }
                ;

                Pending = OldPending;
            }
            return Filter(category);
        }

        /// <summary>
        /// Append two results together, categories are determined by first 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        internal void Add(Result other)
        {
            Add(other,Complete);
        }

        internal void Add(Result other, Category category)
        {
            Tracer.Assert(other.HasCategory(category));
            Tracer.Assert(HasCategory(category));
            IsDirty = true;
            if (category.HasSize) Size += other.Size;
            if (category.HasType) Type = Type.CreatePair(other.Type);
            if (category.HasCode) Code = Code.CreateSequence(other.Code);
            if (category.HasRefs) Refs = Refs.Pair(other.Refs);
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
            Result result = Clone(category);
            result.Add(second);
            return result;
        }

        /// <summary>
        /// Append two results together, categories are determined by first, replaces arg 
        /// </summary>
        /// <param name="result"></param>
        /// <param name="topRef"></param>
        /// <returns></returns>
        public void AddStruct(Result result, Result topRef)
        {
            Add(result.UseWithArg(topRef));
        }

        /// <summary>
        /// Converts the bit array to bit array.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="sourceBitCount">The source count.</param>
        /// <param name="destBitCount">The dest count.</param>
        /// <returns></returns>
        /// [created 30.05.2006 00:17]
        public static Result ConvertBitArrayToBitArray(Category category, int sourceBitCount, int destBitCount)
        {
            if (sourceBitCount == 0 && destBitCount == 0)
                return Reni.Type.Base.CreateVoidResult(category);
            Result result = new Result();
            if (category.HasCode)
            {
                Base codeResult = Base.CreateArg(Size.Create(sourceBitCount));
                if (destBitCount != sourceBitCount)
                    codeResult = codeResult.CreateBitCast(Size.Create(destBitCount));
                result.Code = codeResult;
            }
            if (category.HasRefs) result.Refs = Refs.None();
            return result;
        }

        /// <summary>
        /// Uses the with arg.
        /// </summary>
        /// <param name="resultForArg">The result to use as replacment.</param>
        /// <returns></returns>
        /// [created 30.05.2006 00:25]
        public Result UseWithArg(Result resultForArg)
        {
            if (IsPending)
                return this;

            bool trace = ObjectId == 1490 && resultForArg.ObjectId == 1499;
            StartMethodDump(trace, resultForArg);
            Result result = new Result();
            if (HasSize) result.Size = Size;
            if (HasType) result.Type = Type;
            result.IsDirty = true;
            if (HasCode && resultForArg.HasCode) result.Code = Code.UseWithArg(resultForArg.Code);
            if (HasRefs && resultForArg.HasRefs) result.Refs = Refs.Pair(resultForArg.Refs);
            result.IsDirty = false;
            return ReturnMethodDump(trace, result);
        }

        /// <summary>
        /// Replaces the context ref.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="replacement">The replacement.</param>
        /// <returns></returns>
        internal Result ReplaceAbsoluteContextRef<C>(C context, Base replacement) where C : Context.Base
        {
            if (HasRefs && !Refs.Contains(context))
                return this;

            Result result = new Result();
            if (HasSize) result.Size = Size;
            if (HasType) result.Type = Type;
            result.IsDirty = true;
            if (HasCode) result.Code = Code.ReplaceAbsoluteContextRef(context, replacement);
            if (HasRefs) result.Refs = Refs.Without(context);
            result.IsDirty = false;
            return result;
        }

        /// <summary>
        /// Replaces the context ref.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="replacement">The replacement.</param>
        /// <returns></returns>
        internal Result ReplaceRelativeContextRef<C>(C context, Base replacement) where C : Context.Base
        {
            if (HasRefs && !Refs.Contains(context))
                return this;

            Result result = new Result();
            if (HasSize) result.Size = Size;
            if (HasType) result.Type = Type;
            result.IsDirty = true;
            if (HasCode) result.Code = Code.ReplaceRelativeContextRef(context, replacement);
            if (HasRefs) result.Refs = Refs.Without(context);
            result.IsDirty = false;
            return result;
        }

        /// <summary>
        /// Replaces the refs.
        /// </summary>
        /// <param name="refAlignParam">The ref align param.</param>
        /// <param name="replacement">The replacement.</param>
        /// <returns></returns>
        /// created 31.12.2006 14:50
        internal Result ReplaceRefsForFunctionBody(RefAlignParam refAlignParam, Base replacement)
        {
            if (!HasCode) return this;
            if (Refs.Count == 0) return this;
            Result result = Clone();
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
        public Result SafeList(Result result, Category category)
        {
            Result destructorResult = Type.DestructorHandler(category);
            if (destructorResult.IsEmpty)
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
        public Result CreateRefPlus(Category c, RefAlignParam refAlignParam, Size value)
        {
            Result result = new Result();
            if (c.HasSize) result.Size = Size;
            if (c.HasType) result.Type = Type;
            if (c.HasCode) result.Code = Code.CreateRefPlus(refAlignParam, value);
            if (c.HasRefs) result.Refs = Refs;
            return result;
        }

        /// <summary>
        /// Creates the results for a statement, i. e. respecting temporary results
        /// </summary>
        /// <param name="category"></param>
        /// <param name="tempResult"></param>
        /// <returns></returns>
        public Result CreateStatement(Category category, Result tempResult)
        {
            if (tempResult == null)
                return this;

            Result destructorResult = tempResult.Type.DestructorHandler(category);
            Result finalResult = Clone(category);
            Result moveResult = Type.MoveHandler(category);

            if (category.HasRefs)
                finalResult.Refs = tempResult.Refs
                    .Pair(finalResult.Refs)
                    .Pair(destructorResult.Refs)
                    .Pair(moveResult.Refs);
            if (category.HasCode)
            {
                Base resultCode = tempResult.Code.CreateStatementEndFromIntermediateStorage
                    (
                    Code,
                    destructorResult.Code,
                    moveResult.Code
                    );
                finalResult.Code = resultCode;
            }
            return finalResult;
        }

        /// <summary>
        /// Creates the pending.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <returns></returns>
        /// created 04.02.2007 00:31
        public static Result CreatePending(Category category)
        {
            Result result = new Result();
            if (category.HasSize) result.Size = Size.Pending;
            if (category.HasType) result.Type = Reni.Type.Base.Pending;
            if (category.HasRefs) result.Refs = Refs.Pending;
            if (category.HasCode) result.Code = Base.Pending;
            return result;
        }

        public Result ConvertTo(Type.Base target)
        {
            return Type.ConvertTo(Complete, target).UseWithArg(this);
        }

        public Result CreateUnref(Type.Base type, RefAlignParam refAlignParam)
        {
            return type.CreateResult
                (
                Complete,
                delegate { return Code.CreateDereference(refAlignParam, type.Size); },
                delegate { return Refs; }
                );
        }

        public Result DumpPrintBitSequence()
        {
            return Void.CreateResult
                (
                Complete,
                delegate { return Code.CreateDumpPrint(); },
                delegate { return Refs; }
                );
        }

        internal BitsConst Evaluate()
        {
            Tracer.Assert(Refs.IsNone);
            return Code.Evaluate();
        }

        internal Result EnsureContextRef(Context.Base context)
        {
            if (Type.IsRef)
                return this;

            Result resultAsRef =
                Type
                    .CreateRef(context.RefAlignParam)
                    .CreateResult(Complete, context.TopRefResult)
                    ;
            return resultAsRef;
        }

        internal Result UnProperty(Context.Base context)
        {
            return Type.UnProperty(this, context);
        }

        internal Result PostProcess(Context.Base context)
        {
            return UnProperty(context)
                .Align(context.RefAlignParam.AlignBits);
        }

        internal static Result ConcatPrintResult(Category category, IList<Result> elemResults)
        {
            Result result = Void.CreateResult(category);
            if (category.HasCode)
                result.Code = Reni.Code.Base.CreateDumpPrintText("(");

            for (int i = 0; i < elemResults.Count; i++)
            {
                if (category.HasCode)
                {
                    if (i > 0)
                        result.Code = result.Code.CreateSequence(Reni.Code.Base.CreateDumpPrintText(", "));
                    result.Code = result.Code.CreateSequence(elemResults[i].Code);
                }
                if (category.HasRefs)
                    result.Refs = result.Refs.Pair(elemResults[i].Refs);
            }
            if (category.HasCode)
                result.Code = result.Code.CreateSequence(Reni.Code.Base.CreateDumpPrintText(")"));
            return result;
        }
    }

    /// <summary>
    /// Describes errors, not yet implemented
    /// </summary>
    internal sealed class Error
    {
        private readonly Context.Base _context;
        private readonly Syntax.Base _syntax;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Error"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="syntax">The syntax.</param>
        /// created 29.10.2006 18:23
        internal Error(Context.Base context, Syntax.Base syntax)
        {
            _context = context;
            _syntax = syntax;
        }

        private Error(Error e0, Error e1)
        {
        }

        /// <summary>
        /// asis
        /// </summary>
        /// <param name="e0"></param>
        /// <param name="e1"></param>
        /// <returns></returns>
        public static Error operator +(Error e0, Error e1)
        {
            if (e0 == null)
                return e1;
            if (e1 == null)
                return e0;
            return new Error(e0, e1);
        }
    }
}