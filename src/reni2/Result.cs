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
    [Serializable]
    internal sealed class Result : ReniObject, ITreeNodeSupport, Sequence<Result>.ICombiner<Result>
    {
        private bool _isDirty;
        private Category _pendingCategory;
        private Size _size;
        private TypeBase _type;
        private CodeBase _code;
        private Refs _refs;
        internal PostProcessorForResult PostProcessor;

        public Result()
        {
            PostProcessor = new PostProcessorForResult(this);
            _pendingCategory = new Category();
        }

        private bool HasSize { get { return Size != null; } }
        internal bool HasType { get { return Type != null; } }
        internal bool HasCode { get { return Code != null; } }
        internal bool HasRefs { get { return Refs != null; } }

        [Node]
        private Category PendingCategory { get { return _pendingCategory; } set { _pendingCategory = value; } }

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
            if(!PendingCategory.IsNull)
                result.Add(Service.CreateNamedNode("Pending", "Pending", Dump()));
            if(HasSize)
                result.Add(Service.CreateNamedNode("Size", "Number", Size.FormatForView()));
            if(HasType)
                result.Add(Service.CreateNamedNode("Type", "Type", Type));
            if(HasCode)
                result.Add(Service.CreateNamedNode("Code", "Code", Code));
            if(HasRefs)
                result.Add(Service.CreateNamedNode("Refs", "Refs", Refs.Data));
            return result.ToArray();
        }

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

        internal bool IsPending
        {
            get
            {
                if(CompleteCategory.HasSize)
                    return Size.IsPending;
                if(CompleteCategory.HasType)
                    return Type.IsPending;

                return false;
            }
        }

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

        private void AddPendingCategory(Category category) { _pendingCategory |= category; }

        internal void Update(Result result)
        {
            if(result.HasSize && !result.Size.IsPending)
                _size = result.Size;

            if(result.HasType && !result.Type.IsPending)
                _type = result.Type;

            if(result.HasRefs)
                _refs = result.Refs;

            if(result.HasCode)
                _code = result.Code;

            AssertValid();
        }

        private void Update(Result result, Category category)
        {
            if(category.HasSize)
                _size = result.Size ?? Size.Pending;
            if(category.HasType)
                _type = result.Type ?? TypeBase.Pending;
            if(category.HasRefs)
                _refs = result.Refs ?? Refs.None();
            if(category.HasCode)
                _code = result.Code;
            AssertValid();
        }

        internal Result Filter(Category category)
        {
            if(category == CompleteCategory)
                return this;

            var result = new Result();
            result.Update(this, category);
            return result;
        }

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
            return r;
        }

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

        internal Result Clone() { return Clone(CompleteCategory); }

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
            if(category.HasSize && !CompleteCategory.HasSize)
                return false;
            if(category.HasType && !CompleteCategory.HasType)
                return false;
            if(category.HasCode && !CompleteCategory.HasCode)
                return false;
            if(category.HasRefs && !CompleteCategory.HasRefs)
                return false;
            return true;
        }

        //[DebuggerHidden]
        internal Result AddCategories(Category category, ContextBase context, ICompileSyntax syntax)
        {
            var oldPendingCategory = PendingCategory;
            var notPendingCategory = category - oldPendingCategory;
            var oldCompleteCategory = CompleteCategory;
            var notCompleteCategory = notPendingCategory - oldCompleteCategory;
            if(!notCompleteCategory.IsNull)
            {
                AddPendingCategory(notCompleteCategory);

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

                PendingCategory = oldPendingCategory;
            }
            var filteredResult = Filter(category);
            Tracer.Assert(filteredResult.CompleteCategory == category, string.Format("syntax={2}\ncategory={0}\nResult={1}", category, filteredResult.Dump(), syntax.DumpShort()));
            return filteredResult;
        }

        internal void AssertComplete(Category category, ICompileSyntax syntaxForDump) { Tracer.Assert(1, HasCategory(category), string.Format("syntax={2}\ncategory={0}\nResult={1}", category, Dump(), syntaxForDump.DumpShort())); }

        internal void AssertComplete(Category category) { Tracer.Assert(1, HasCategory(category), string.Format("category={0}\nResult={1}", category, Dump())); }

        internal void Add(Result other) { Add(other, CompleteCategory); }

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
            IsDirty = false;
        }

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

        internal Result UseWithArg(Result resultForArg)
        {
            if(IsPending)
                return this;

            var trace = ObjectId == 1490 && resultForArg.ObjectId == 1499;
            StartMethodDump(trace, resultForArg);
            var result = new Result {Size = Size, Type = Type};
            if(HasCode && resultForArg.HasCode)
                result.Code = Code.UseWithArg(resultForArg.Code);
            if(HasRefs && resultForArg.HasRefs)
                result.Refs = Refs.CreateSequence(resultForArg.Refs);
            return ReturnMethodDump(trace, result);
        }

        internal Result ReplaceAbsoluteContextRef<C>(C context, Result replacement) where C : IRefInCode
        {
            if(IsPending)
                return this;

            if(HasRefs && !Refs.Contains(context))
                return this;

            var result = new Result {Size = Size, Type = Type, IsDirty = true};
            if(HasCode)
                result.Code = Code.ReplaceAbsoluteContextRef(context, replacement.Code);
            if(HasRefs)
                result.Refs = Refs.Without(context).CreateSequence(replacement.Refs);
            result.IsDirty = false;
            return result;
        }

        internal Result ReplaceRelativeContextRef<C>(C context, CodeBase replacement) where C : IRefInCode
        {
            if(HasRefs && !Refs.Contains(context))
                return this;

            var result = new Result {Size = Size, Type = Type};
            if(HasCode)
                result.Code = Code.ReplaceRelativeContextRef(context, replacement);
            if(HasRefs)
                result.Refs = Refs.Without(context);
            return result;
        }

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

        internal Result SafeList(Result result, Category category)
        {
            var destructorResult = Type.Destructor(category);
            if(destructorResult.IsEmpty)
                return CreateSequence(result, category);

            NotImplementedMethod(result, category);
            throw new NotImplementedException();
        }

        internal Result CreateRefPlus(Category c, RefAlignParam refAlignParam, Size value)
        {
            var result = Clone();
            if(c.HasCode)
                result.Code = Code.CreateRefPlus(refAlignParam, value);
            return result;
        }

        internal Result CreateStatement(Category category, RefAlignParam refAlignParam)
        {
            if (!category.HasCode && !category.HasRefs)
                return this;

            var result = Clone(category);
            var copier = Type.Copier(category);
            if(category.HasCode)
                result.Code = Code.CreateStatement(copier.Code,refAlignParam);
            if (category.HasRefs)
                result.Refs = result.Refs.CreateSequence(copier.Refs);
            return result;
        }

        internal static Result CreatePending(Category category)
        {
            var result = new Result();
            if(category.HasSize)
                result.Size = Size.Pending;
            if(category.HasType)
                result.Type = TypeBase.Pending;
            if(category.HasRefs)
                result.Refs = Refs.None();
            Tracer.Assert(!category.HasCode);
            return result;
        }

        internal Result ConvertTo(TypeBase target) { return Type.Conversion(CompleteCategory, target).UseWithArg(this); }

        internal Result CreateUnref(TypeBase type, RefAlignParam refAlignParam)
        {
            return type.CreateResult
                (
                CompleteCategory,
                () => Code.CreateDereference(refAlignParam, type.Size),
                () => Refs
                );
        }

        internal Result DumpPrintBitSequence()
        {
            return Reni.Type.Void.CreateResult
                (
                CompleteCategory,
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
            if(IsPending || CompleteCategory == Category.Refs)
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

        internal Result CreateAutomaticRefResult(Category category, AutomaticRef target)
        {
            var destructor = Type.Destructor(category);
            return target.CreateResult(
                category,
                () => CodeBase.CreateInternalRef(target.RefAlignParam, Code, destructor.Code),
                () => Refs + destructor.Refs
                );
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
