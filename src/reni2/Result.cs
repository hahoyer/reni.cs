﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;
using Reni.Code;
using Reni.Context;
using Reni.Syntax;
using Reni.Type;

namespace Reni
{
    [Serializable]
    internal sealed class Result : ReniObject, ITreeNodeSupport
    {
        private static int _nextObjectId = 1;
        private bool _isDirty;
        private Size _size;
        private TypeBase _type;
        private CodeBase _code;
        private Refs _refs;
        internal readonly PostProcessorForResult PostProcessor;

        public Result(): base(_nextObjectId++)
        {
            PostProcessor = new PostProcessorForResult(this);
            PendingCategory = new Category();
        }

        private bool HasSize { get { return Size != null; } }
        internal bool HasType { get { return Type != null; } }
        internal bool HasCode { get { return Code != null; } }
        internal bool HasRefs { get { return Refs != null; } }

        [Node, DumpData(false)]
        internal Category PendingCategory;

        public Category CompleteCategory { get { return new Category(HasSize, HasType, HasCode, HasRefs); } }

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

        TreeNode[] ITreeNodeSupport.CreateNodes()
        {
            var result = new List<TreeNode>();
            if(!PendingCategory.IsNone)
                result.Add(Dump().CreateNamedNode("Pending", "Pending"));
            if(HasSize)
                result.Add(Size.FormatForView().CreateNamedNode("Size", "Number"));
            if(HasType)
                result.Add(Type.CreateNamedNode("Type", "Type"));
            if(HasCode)
                result.Add(Code.CreateNamedNode("Code", "Code"));
            if(HasRefs)
                result.Add(Refs.Data.CreateNamedNode("Refs", "Refs"));
            return result.ToArray();
        }

        internal Size FindSize
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

        internal Refs FindRefs
        {
            get
            {
                if(HasRefs)
                    return Refs;
                if(HasCode)
                    return Code.Refs;
                return null;
            }
        }

        internal Refs SmartRefs
        {
            get
            {
                var result = FindRefs;
                if(result == null)
                {
                    DumpMethodWithBreak("No approriate result property defined");
                    Debugger.Break();
                }
                return result;
            }
        }

        internal static Error Error { get { return null; } }

        private bool IsDirty
        {
            get { return _isDirty; }
            set
            {
                _isDirty = value;
                AssertValid();
            }
        }

        internal bool IsEmpty
        {
            get
            {
                if(CompleteCategory.HasSize && !Size.IsZero)
                    return false;
                if(CompleteCategory.HasType && !Type.IsVoid)
                    return false;
                if(CompleteCategory.HasCode && !Code.IsEmpty)
                    return false;
                if(CompleteCategory.HasRefs && !Refs.IsNone)
                    return false;
                return true;
            }
        }

        internal bool IsCodeLess
        {
            get
            {
                if(CompleteCategory.HasSize && !Size.IsZero)
                    return false;
                if(CompleteCategory.HasCode && !Code.IsEmpty)
                    return false;
                if(CompleteCategory.HasRefs && !Refs.IsNone)
                    return false;
                return true;
            }
        }

        internal bool HasArg { get { return HasCode && Code.HasArg; } }


        public override string DumpData()
        {
            var result = "";
            result += "PendingCategory=" + PendingCategory.Dump();
            result += "\n";
            result += "CompleteCategory=" + CompleteCategory.Dump();
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

        internal void Update(Result result)
        {
            if(result.HasSize)
                _size = result.Size;

            if(result.HasType)
                _type = result.Type;

            if(result.HasRefs)
                _refs = result.Refs;

            if(result.HasCode)
                _code = result.Code;

            AssertValid();
        }

        private Result Filter(Category category)
        {
            var result = new Result
                             {
                                 PendingCategory = PendingCategory & category
                             };

            if(category.HasSize)
                result._size = Size;
            if(category.HasType)
                result._type = Type;
            if(category.HasRefs)
                result._refs = Refs;
            if(category.HasCode)
                result._code = Code;
            result.AssertValid();
            return result;
        }

        internal Result Align(int alignBits)
        {
            var size = FindSize;
            if(size == null)
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
            return r;
        }

        private Result Clone(Category category)
        {
            var r = new Result {PendingCategory = PendingCategory & category};
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

        internal Result Clone() { return new Result {PendingCategory = PendingCategory, Size = Size, Type = Type, Code = Code, Refs = Refs}; }

        private void AssertValid()
        {
            if(IsDirty)
                return;

            var size = FindSize;
            if(size != null)
            {
                if(HasSize && Size != size)
                    Tracer.AssertionFailed(1, @"Size==size", ()=>"Size differs " + Dump());
                if(HasType && Type.Size != size)
                    Tracer.AssertionFailed(1, @"Type.Size==size", () => "Type size differs " + Dump());
                if(HasCode && Code.Size != size)
                    Tracer.AssertionFailed(1, @"Code.Size==size", () => "Code size differs " + Dump());
            }

            if(HasRefs && HasCode && !Refs.Contains(Code.RefsImplementation))
                Tracer.AssertionFailed(1, @"Refs.Contains(codeRefs)", () => "Code and Refs differ " + Dump());
        }

        [DebuggerHidden]
        internal void AddCategories(ContextBase context, Category category, ICompileSyntax syntax)
        {
            var trace = context.ObjectId == -17 && category.HasRefs && IsObjectId(syntax, 1192);
            StartMethodDumpWithBreak(trace, context, category, syntax);
            var localCateogory = category - CompleteCategory - PendingCategory;
            
            if(localCateogory.HasSize && FindSize != null)
            {
                _size = FindSize;
                localCateogory -= Category.Size;
            }

            if (localCateogory.HasRefs && FindRefs != null)
            {
                _refs = FindRefs;
                localCateogory -= Category.Refs;
            }

            InternalAddCategories(context, localCateogory, syntax);
            TreatPendingCategories(context, category - CompleteCategory, syntax);
            ReturnMethodDumpWithBreak(trace);
        }

        private void TreatPendingCategories(ContextBase context, Category category, ICompileSyntax syntax)
        {
            if(category.IsNone)
                return;

            var result = context.PendingResult(category, syntax);
            Tracer.Assert(result.CompleteCategory == category);
            Update(result);
        }

        [DebuggerHidden]
        private void InternalAddCategories(ContextBase context, Category category, ICompileSyntax syntax)
        {
            if(category.IsNone)
                return;

            var oldPendingCategory = PendingCategory;
            PendingCategory |= category;

            var result = syntax.Result(context, category);
            context.AssertCorrectRefs(this);
            result.AssertComplete(category, syntax);
            Update(result);
            PendingCategory = oldPendingCategory;
        }

        private void AssertComplete(Category category, ICompileSyntax syntaxForDump)
        {
            Tracer.Assert
                (
                1,
                category <= (CompleteCategory | PendingCategory),
                () => string.Format("syntax={2}\ncategory={0}\nResult={1}", category, Dump(), syntaxForDump.DumpShort())
                );
        }

        private void Add(Result other) { Add(other, CompleteCategory); }

        private void Add(Result other, Category category)
        {
            Tracer.Assert(category <= other.CompleteCategory);
            Tracer.Assert(category <= CompleteCategory);
            IsDirty = true;
            if(category.HasSize)
                Size += other.Size;
            if(category.HasType)
                Type = Type.CreatePair(other.Type);
            if(category.HasCode)
                Code = Code.CreateSequence(other.Code);
            if(category.HasRefs)
                Refs = Refs.CreateSequence(other.Refs);
            IsDirty = false;
        }

        private Result CreateSequence(Result second, Category category)
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

        internal Result ReplaceArg(Result resultForArg)
        {
            var trace = ObjectId == 1490 && resultForArg.ObjectId == 1499;
            StartMethodDump(trace, resultForArg);
            var result = new Result {Size = Size, Type = Type};
            if(HasCode && resultForArg.HasCode)
                result.Code = Code.ReplaceArg(resultForArg.Code);
            if(HasRefs && resultForArg.HasRefs)
                result.Refs = Refs.CreateSequence(resultForArg.Refs);
            return ReturnMethodDump(trace, result);
        }


        /// <summary>
        /// Replaces the absolute refInCode ref.
        /// </summary>
        /// <typeparam name="TRefInCode"></typeparam>
        /// <param name="refInCode">The refInCode.</param>
        /// <param name="replacement">The replacement. Must not contain a reference that varies when walking along code tree.</param>
        /// <returns></returns>
        internal Result ReplaceAbsolute<TRefInCode>(TRefInCode refInCode, Func<Result> replacement)
            where TRefInCode : IRefInCode
        {
            if(HasRefs && !Refs.Contains(refInCode))
                return this;

            var result = new Result {Size = Size, Type = Type, IsDirty = true};
            if(HasCode)
                result.Code = Code.ReplaceAbsolute(refInCode, ()=>replacement().Code);
            if(HasRefs)
                result.Refs = Refs.Without(refInCode).CreateSequence(replacement().Refs);
            result.IsDirty = false;
            return result;
        }

        /// <summary>
        /// Replaces the relative refInCode ref.
        /// </summary>
        /// <typeparam name="TRefInCode"></typeparam>
        /// <param name="refInCode">The refInCode.</param>
        /// <param name="replacement">The replacement. Should be a reference that varies when walking along code tree.</param>
        /// <returns></returns>
        internal Result ReplaceRelative<TRefInCode>(TRefInCode refInCode, Func<CodeBase> replacement)
            where TRefInCode : IRefInCode
        {
            if(HasRefs && !Refs.Contains(refInCode))
                return this;

            var result = new Result {Size = Size, Type = Type};
            if(HasCode)
                result.Code = Code.ReplaceRelative(refInCode, replacement);
            if(HasRefs)
                result.Refs = Refs.Without(refInCode);
            return result;
        }

        internal Result ReplaceObjectRefByArg(RefAlignParam refAlignParam, TypeBase objectType) { return objectType.ReplaceObjectRefByArg(this, refAlignParam); }

        internal Result ReplaceRefsForFunctionBody(RefAlignParam refAlignParam, CodeBase replacement)
        {
            if(!HasCode)
                return this;
            if(SmartRefs.Count == 0)
                return this;
            var result = Clone();
            result.IsDirty = true;
            result.Code = SmartRefs.ReplaceRefsForFunctionBody(Code, refAlignParam, replacement);
            result.Refs = Refs.None();
            result.IsDirty = false;
            return result;
        }

        internal Result SafeList(Result result, Category category)
        {
            var destructorResult = Type.Destructor(category);
            if(destructorResult.IsEmpty)
                return CreateSequence(result, category);

            NotImplementedMethod(result, category);
            throw new NotImplementedException();
        }

        internal Result CreateRefPlus(Category c, RefAlignParam refAlignParam, Size value, string reason)
        {
            var result = Clone();
            if(c.HasCode)
                result.Code = Code.CreateRefPlus(refAlignParam, value, reason);
            return result;
        }

        internal Result CreateLocalBlock(Category category, RefAlignParam refAlignParam)
        {
            if(!category.HasCode && !category.HasRefs)
                return this;

            var result = Clone(category);
            var copier = Type.Copier(category);
            if(category.HasCode)
                result.Code = Code.CreateLocalBlock(copier.Code, refAlignParam);
            if(category.HasRefs)
                result.Refs = Refs.CreateSequence(copier.Refs);
            return result;
        }

        internal Result ConvertTo(TypeBase target)
        {
            return Type.Conversion(CompleteCategory, target).ReplaceArg(this);
        }

        internal Result CreateUnref(TypeBase type, RefAlignParam refAlignParam)
        {
            return type.CreateResult
                (
                CompleteCategory,
                () => Code.CreateDereference(refAlignParam, type.Size),
                () => Refs
                );
        }

        internal BitsConst Evaluate()
        {
            Tracer.Assert(Refs.IsNone);
            return Code
                .Serialize(false)
                .Evaluate();
        }

        internal Result AutomaticDereference()
        {
            if(CompleteCategory == Category.Refs)
                return this;

            return Type.AutomaticDereference(this);
        }

        internal static Result ConcatPrintResult(Category category, IList<Result> elemResults)
        {
            var result = TypeBase.CreateVoidResult(category);
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

        [DebuggerHidden]
        public static Result operator &(Result result, Category category) { return result.Filter(category); }

        [DebuggerHidden]
        public static Result operator |(Result aResult, Result bResult)
        {
            Tracer.Assert((aResult.CompleteCategory & bResult.CompleteCategory).IsNone);
            var result = aResult.Clone();
            result.Update(bResult);
            return result;
        }

        internal Result(Category category, Func<Size> getSize, Func<TypeBase> getType, Func<CodeBase> getCode, Func<Refs> getRefs)
            : this()
        {
            if(category.HasSize)
                _size = getSize();
            if(category.HasType)
                _type = getType();
            if(category.HasCode)
                _code = getCode();
            if(category.HasRefs)
                _refs = getRefs();
        }

        internal Result(Category category, Func<Size> getSize, Func<CodeBase> getCode, Func<Refs> getRefs)
            : this()
        {
            if (category.HasSize)
                _size = getSize();
            if (category.HasCode)
                _code = getCode();
            if (category.HasRefs)
                _refs = getRefs();
        }

        internal Result StripFunctional()
        {
            if(!HasType)
                return this;
            return Type.StripFunctional().CreateResult(this);
        }

        internal Result ConvertToBitSequence(Category category)
        {
            return Type
                .ConvertToBitSequence(category)
                .ReplaceArg(this);
        }

        internal Result ConvertToAsRef(Category category, Reference target)
        {
            return Type
                .ConvertToAsRef(category, target)
                .ReplaceArg(this);
        }

        public Result CreateLocalReferenceResult(RefAlignParam refAlignParam) {
            return Type
                .CreateLocalReferenceResult(CompleteCategory, refAlignParam)
                .ReplaceArg(this);
        }
    }

    internal sealed class Error
    {
        private readonly ContextBase _context;
        private readonly ICompileSyntax _syntax;

        internal Error(ContextBase context, ICompileSyntax syntax)
        {
            _context = context;
            _syntax = syntax;
        }

        private Error(Error e0, Error e1) { }

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