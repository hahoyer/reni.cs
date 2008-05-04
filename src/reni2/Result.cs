using System;
using System.Collections.Generic;
using System.Diagnostics;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper.TreeViewSupport;
using Reni.Context;

namespace Reni
{
    /// <summary>
    /// Result of a visitor request
    /// </summary>
    internal sealed class Result : ReniObject
    {
        private Code.CodeBase _code;
        private bool _isDirty;
        private Category _pending;
        private Refs _refs;
        private Size _size;
        private Type.TypeBase _type;

        /// <summary>
        /// ctor
        /// </summary>
        public Result()
        {
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
        public Type.TypeBase Type
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
        internal Code.CodeBase Code
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
        internal static Error Error { get { return null; } set { } }

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
            return result;
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
            if(r.HasSize && !r.Size.IsPending)
                _size = r.Size;

            if(r.HasType && !r.Type.IsPending)
                _type = r.Type;

            if(r.HasRefs && !r.Refs.IsPending)
                _refs = r.Refs;

            if(r.HasCode && !r.Code.IsPending)
                _code = r.Code;
        }

        private void Update(Result r, Category c)
        {
            if(c.HasSize)
                _size = r.Size ?? Size.Pending;
            if(c.HasType)
                _type = r.Type ?? Reni.Type.TypeBase.Pending;
            if(c.HasRefs)
                _refs = r.Refs ?? Refs.Pending;
            if(c.HasCode)
                _code = r.Code ?? Reni.Code.CodeBase.Pending;
        }

        private Result Filter(Category category)
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
            if(size == null)
            {
                var r = new Result();
                if(HasRefs)
                    r.Refs = Refs;
                return r;
            }

            if(size.IsPending)
                return this;

            var alignedSize = size.Align(alignBits);
            if(alignedSize != size)
            {
                var r = new Result();
                if(HasSize)
                    r.Size = alignedSize;
                if(HasType)
                    r.Type = Type.CreateAlign(alignBits);
                if(HasCode)
                    r.Code = Code.CreateBitCast(alignedSize);
                if(HasRefs)
                    r.Refs = Refs;
                return r;
            }
            return this;
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
            return r;
        }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns></returns>
        /// created 19.11.2006 22:13
        internal Result Clone()
        {
            return Clone(Complete);
        }

        private void AssertValid()
        {
            if(IsDirty)
                return;

            var size = FindSize;
            if(size != null)
            {
                if(HasSize)
                {
                    if(!(Size == size))
                    {
                        Tracer.AssertionFailed(1, @"Size==size", "Size differs " + Dump());
                        Debugger.Break();
                    }
                }
                if(HasType)
                {
                    if(!(Type.Size == size))
                    {
                        Tracer.AssertionFailed(1, @"Type.Size==size", "Type size differs " + Dump());
                        Debugger.Break();
                    }
                }
                if(HasCode)
                {
                    if(!(Code.Size == size))
                    {
                        Tracer.AssertionFailed(1, @"Code.Size==size", "Code size differs " + Dump());
                        Debugger.Break();
                    }
                }
            }

            if(HasRefs && HasCode)
            {
                if(!Refs.Contains(Code.Refs))
                {
                    Tracer.AssertionFailed(1, @"Refs.Contains(codeRefs)", "Code and Refs differ " + Dump());
                    Debugger.Break();
                }
            }
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
        internal Result Visit(Category category, ContextBase context, Syntax.SyntaxBase syntax)
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
                    var RSub = syntax.VirtVisit(context, notCompleteCategory);
                    Update(RSub);
                }
                catch
                {
                    Tracer.FlaggedLine("CCompilerMsg(*mSyntaxKey->TokenClass, \"aftereffect\").Show()");
                    throw;
                }

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
            Add(other, Complete);
        }

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
                Refs = Refs.Pair(other.Refs);
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

        /// <summary>
        /// Append two results together, categories are determined by first, replaces arg 
        /// </summary>
        /// <param name="result"></param>
        /// <param name="topRef"></param>
        /// <returns></returns>
        internal void AddStruct(Result result, Result topRef)
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
        internal static Result ConvertBitArrayToBitArray(Category category, int sourceBitCount, int destBitCount)
        {
            if(sourceBitCount == 0 && destBitCount == 0)
                return Reni.Type.TypeBase.CreateVoidResult(category);
            var result = new Result();
            if(category.HasCode)
            {
                var codeResult = Reni.Code.CodeBase.CreateArg(Size.Create(sourceBitCount));
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
            var result = new Result();
            if(HasSize)
                result.Size = Size;
            if(HasType)
                result.Type = Type;
            result.IsDirty = true;
            if(HasCode && resultForArg.HasCode)
                result.Code = Code.UseWithArg(resultForArg.Code);
            if(HasRefs && resultForArg.HasRefs)
                result.Refs = Refs.Pair(resultForArg.Refs);
            result.IsDirty = false;
            return ReturnMethodDump(trace, result);
        }

        /// <summary>
        /// Replaces the context ref.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="replacement">The replacement.</param>
        /// <returns></returns>
        internal Result ReplaceAbsoluteContextRef<C>(C context, Result replacement) where C : ContextBase
        {
            if(HasRefs && !Refs.Contains(context))
                return this;

            var result = new Result();
            if(HasSize)
                result.Size = Size;
            if(HasType)
                result.Type = Type;
            result.IsDirty = true;
            if(HasCode)
                result.Code = Code.ReplaceAbsoluteContextRef(context, replacement.Code);
            if(HasRefs)
                result.Refs = Refs.Without(context).Pair(replacement.Refs);
            result.IsDirty = false;
            return result;
        }

        /// <summary>
        /// Replaces the context ref.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="replacement">The replacement.</param>
        /// <returns></returns>
        internal Result ReplaceRelativeContextRef<C>(C context, Code.CodeBase replacement) where C : ContextBase
        {
            if(HasRefs && !Refs.Contains(context))
                return this;

            var result = new Result();
            if(HasSize)
                result.Size = Size;
            if(HasType)
                result.Type = Type;
            result.IsDirty = true;
            if(HasCode)
                result.Code = Code.ReplaceRelativeContextRef(context, replacement);
            if(HasRefs)
                result.Refs = Refs.Without(context);
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
        internal Result ReplaceRefsForFunctionBody(RefAlignParam refAlignParam, Code.CodeBase replacement)
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
            var result = new Result();
            if(c.HasSize)
                result.Size = Size;
            if(c.HasType)
                result.Type = Type;
            if(c.HasCode)
                result.Code = Code.CreateRefPlus(refAlignParam, value);
            if(c.HasRefs)
                result.Refs = Refs;
            return result;
        }

        /// <summary>
        /// Creates the results for a statement, i. e. respecting temporary results
        /// </summary>
        /// <param name="category"></param>
        /// <param name="tempResult"></param>
        /// <returns></returns>
        internal Result CreateStatement(Category category, Result tempResult)
        {
            if(tempResult == null)
                return this;

            var destructorResult = tempResult.Type.DestructorHandler(category);
            var finalResult = Clone(category);
            var moveResult = Type.MoveHandler(category);

            if(category.HasRefs)
                finalResult.Refs = tempResult.Refs
                    .Pair(finalResult.Refs)
                    .Pair(destructorResult.Refs)
                    .Pair(moveResult.Refs);
            if(category.HasCode)
            {
                var resultCode = tempResult.Code.CreateStatementEndFromIntermediateStorage
                    (
                    Code,
                    destructorResult.Code,
                    moveResult.Code
                    );
                finalResult.Code = resultCode;
            }
            return finalResult;
        }

        internal static Result CreatePending(Category category)
        {
            var result = new Result();
            if(category.HasSize)
                result.Size = Size.Pending;
            if(category.HasType)
                result.Type = Reni.Type.TypeBase.Pending;
            if(category.HasRefs)
                result.Refs = Refs.Pending;
            if(category.HasCode)
                result.Code = Reni.Code.CodeBase.Pending;
            return result;
        }

        internal Result ConvertTo(Type.TypeBase target)
        {
            return Type.ConvertTo(Complete, target).UseWithArg(this);
        }

        internal Result CreateUnref(Type.TypeBase type, RefAlignParam refAlignParam)
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
            return Code.Evaluate();
        }

        internal Result EnsureContextRef(ContextBase context)
        {
            if(Type.IsRef)
                return this;

            var resultAsRef =
                Type
                    .CreateRef(context.RefAlignParam)
                    .CreateResult(Complete, context.TopRefResult)
                ;
            return resultAsRef;
        }

        internal Result UnProperty(ContextBase context)
        {
            return Type.UnProperty(this, context);
        }

        internal Result PostProcess(ContextBase context)
        {
            return UnProperty(context)
                .Align(context.RefAlignParam.AlignBits);
        }

        internal static Result ConcatPrintResult(Category category, IList<Result> elemResults)
        {
            var result = Reni.Type.Void.CreateResult(category);
            if(category.HasCode)
                result.Code = Reni.Code.CodeBase.CreateDumpPrintText("(");

            for(var i = 0; i < elemResults.Count; i++)
            {
                if(category.HasCode)
                {
                    if(i > 0)
                        result.Code = result.Code.CreateSequence(Reni.Code.CodeBase.CreateDumpPrintText(", "));
                    result.Code = result.Code.CreateSequence(elemResults[i].Code);
                }
                if(category.HasRefs)
                    result.Refs = result.Refs.Pair(elemResults[i].Refs);
            }
            if(category.HasCode)
                result.Code = result.Code.CreateSequence(Reni.Code.CodeBase.CreateDumpPrintText(")"));
            return result;
        }

        #region Nested type: GetCode

        ///<summary>
        /// Delegate that returns code
        ///</summary>
        internal delegate Code.CodeBase GetCode();

        #endregion

        #region Nested type: GetRefs

        ///<summary>
        /// Delegate that returns refs
        ///</summary>
        internal delegate Refs GetRefs();

        #endregion
    }

    /// <summary>
    /// Describes errors, not yet implemented
    /// </summary>
    internal sealed class Error
    {
        private readonly ContextBase _context;
        private readonly Syntax.SyntaxBase _syntax;

        /// <summary>
        /// Initializes a new instance of the <see cref="Error"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="syntax">The syntax.</param>
        /// created 29.10.2006 18:23
        internal Error(ContextBase context, Syntax.SyntaxBase syntax)
        {
            _context = context;
            _syntax = syntax;
        }

        private Error(Error e0, Error e1) {}

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
        internal Syntax.SyntaxBase Syntax { get { return _syntax; } }
    }
}