using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using hw.Debug;
using hw.Forms;
using hw.Helper;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Struct;
using Reni.Type;

namespace Reni
{
    sealed class Result : DumpableObject, ITreeNodeSupport, IAggregateable<Result>
    {
        static int _nextObjectId = 1;
        bool _isDirty;
        bool? _hllw;
        Size _size;
        TypeBase _type;
        CodeBase _code;
        CodeArgs _exts;
        Category _pendingCategory;

        internal Result()
            : base(_nextObjectId++)
        {
            _pendingCategory = Category.None;
            StopByObjectId(-88);
        }

        internal Result
            (
            Category category,
            Func<bool> getHllw = null,
            Func<Size> getSize = null,
            Func<TypeBase> getType = null,
            Func<CodeBase> getCode = null,
            Func<CodeArgs> getExts = null,
            Root rootContext = null)
            : this()
        {
            var hllw = getHllw == null ? null : new ValueCache<bool>(getHllw);
            var size = getSize == null ? null : new ValueCache<Size>(getSize);
            var type = getType == null ? null : new ValueCache<TypeBase>(getType);
            var code = getCode == null ? null : new ValueCache<CodeBase>(getCode);
            var exts = getExts == null ? null : new ValueCache<CodeArgs>(getExts);

            if(category.HasType)
                _type = ObtainType(hllw, size, type, code, rootContext);

            if(category.HasCode)
                _code = ObtainCode(hllw, size, type, code);

            if(category.HasSize)
                _size = ObtainSize(hllw, size, type, code);

            if(category.HasExts)
                _exts = ObtainExts(hllw, size, type, code, exts);

            if(category.HasHllw)
                _hllw = ObtainHllw(hllw, size, type, code);

            AssertValid();
        }

        CodeBase ObtainCode
            (
            ValueCache<bool> getHllw,
            ValueCache<Size> getSize,
            ValueCache<TypeBase> getType,
            ValueCache<CodeBase> getCode)
        {
            if(getCode != null)
                return getCode.Value;
// ReSharper disable ExpressionIsAlwaysNull
            var hllw = TryObtainHllw(getHllw, getSize, getType, getCode);
// ReSharper restore ExpressionIsAlwaysNull
            if(hllw == true)
                return CodeBase.Void;
            Tracer.AssertionFailed("Code cannot be determined", ToString);
            return null;
        }

        TypeBase ObtainType
            (
            ValueCache<bool> getHllw,
            ValueCache<Size> getSize,
            ValueCache<TypeBase> getType,
            ValueCache<CodeBase> getCode,
            Root rootContext)
        {
            if(getType != null)
                return getType.Value;
// ReSharper disable ExpressionIsAlwaysNull
            var hllw = TryObtainHllw(getHllw, getSize, getType, getCode);
// ReSharper restore ExpressionIsAlwaysNull
            if(hllw == true)
                return rootContext.VoidType;
            Tracer.AssertionFailed("Type cannot be determned", ToString);
            return null;
        }

        Size ObtainSize
            (
            ValueCache<bool> getHllw,
            ValueCache<Size> getSize,
            ValueCache<TypeBase> getType,
            ValueCache<CodeBase> getCode)
        {
            var result = TryObtainSize(getHllw, getSize, getType, getCode);
            Tracer.Assert(result != null, () => "Size cannot be determned " + ToString());
            return result;
        }

        static Size TryObtainSize
            (
            ValueCache<bool> getHllw,
            ValueCache<Size> getSize,
            ValueCache<TypeBase> getType,
            ValueCache<CodeBase> getCode)
        {
            if(getSize != null)
                return getSize.Value;
            if(getType != null)
                return getType.Value.Size;
            if(getCode != null)
                return getCode.Value.Size;
            if(getHllw != null && getHllw.Value)
                return Size.Zero;
            return null;
        }

        bool ObtainHllw
            (
            ValueCache<bool> getHllw,
            ValueCache<Size> getSize,
            ValueCache<TypeBase> getType,
            ValueCache<CodeBase> getCode)
        {
            var result = TryObtainHllw(getHllw, getSize, getType, getCode);
            if(result != null)
                return result.Value;
            Tracer.AssertionFailed("Datalessness cannot be determned", ToString);
            return false;
        }

        static bool? TryObtainHllw
            (
            ValueCache<bool> getHllw,
            ValueCache<Size> getSize,
            ValueCache<TypeBase> getType,
            ValueCache<CodeBase> getCode)
        {
            if(getHllw != null)
                return getHllw.Value;
            if(getSize != null)
                return getSize.Value.IsZero;
            if(getType != null)
                return getType.Value.Hllw;
            if(getCode != null)
                return getCode.Value.IsEmpty;
            return null;
        }

        CodeArgs ObtainExts
            (
            ValueCache<bool> getHllw,
            ValueCache<Size> getSize,
            ValueCache<TypeBase> getType,
            ValueCache<CodeBase> getCode,
            ValueCache<CodeArgs> getArgs)
        {
            if(getArgs != null)
                return getArgs.Value;
            if(getCode != null)
                return getCode.Value.Exts;
// ReSharper disable ExpressionIsAlwaysNull
            if(TryObtainHllw(getHllw, getSize, getType, getCode) == true)
// ReSharper restore ExpressionIsAlwaysNull
                return CodeArgs.Void();

            Tracer.AssertionFailed("CodeArgs cannot be determned", ToString);
            return null;
        }

        internal bool HasSize { get { return Size != null; } }
        internal bool HasType { get { return Type != null; } }
        internal bool HasCode { get { return Code != null; } }
        internal bool HasExts { get { return Exts != null; } }
        internal bool HasHllw { get { return _hllw != null; } }

        [Node]
        [EnableDumpWithExceptionPredicate]
        public Category CompleteCategory { get { return Category.CreateCategory(HasHllw, HasSize, HasType, HasCode, HasExts); } }

        [Node]
        [DebuggerHidden]
        public bool? Hllw
        {
            get { return _hllw; }
            set
            {
                _hllw = value;
                AssertValid();
            }
        }

        [Node]
        [DebuggerHidden]
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
        [DebuggerHidden]
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
            [DebuggerHidden] get { return _code; }
            [DebuggerHidden]
            set
            {
                _code = value;
                AssertValid();
            }
        }

        [Node]
        [DebuggerHidden]
        public CodeArgs Exts
        {
            get { return _exts; }
            set
            {
                _exts = value;
                AssertValid();
            }
        }

        IEnumerable<TreeNode> ITreeNodeSupport.CreateNodes() { return TreeNodes; }

        [DisableDump]
        internal IEnumerable<TreeNode> TreeNodes
        {
            get
            {
                if(_pendingCategory.HasAny)
                    yield return Dump().CreateNamedNode("Pending", "Pending");
                if(HasHllw)
                    yield return Hllw.CreateNamedNode("Hllw", "Logical");
                if(HasSize)
                    yield return Size.FormatForView().CreateNamedNode("Size", "Number");
                if(HasType)
                    yield return Type.CreateNamedNode("Type", "Type");
                if(HasCode)
                    yield return Code.CreateNamedNode("Code", "Code");
                if(HasExts)
                    yield return Exts.Data.CreateNamedNode("Exts", "Exts");
            }
        }

        internal bool? FindHllw
        {
            get
            {
                if(HasHllw)
                    return _hllw;
                var size = FindSize;
                if(size == null)
                    return null;
                return size.IsZero;
            }
        }

        bool? QuickFindHllw
        {
            get
            {
                if(HasHllw)
                    return _hllw;
                var size = QuickFindSize;
                if(size == null)
                    return null;
                return size.IsZero;
            }
        }

        internal bool SmartHllw
        {
            get
            {
                var result = FindHllw;
                if(result == null)
                {
                    DumpMethodWithBreak("No approriate result property defined");
                    Debugger.Break();
                    return false;
                }
                return result.Value;
            }
        }

        internal Size FindSize
        {
            get
            {
                if(HasSize)
                    return Size;
                if(HasCode)
                    return Code.Size;
                if(HasType)
                    return Type.Size;
                return null;
            }
        }

        Size QuickFindSize
        {
            get
            {
                if(HasSize)
                    return Size;
                if(HasCode)
                    return Code.Size;
                if(HasType && Type.HasQuickSize)
                    return Type.Size;
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

        internal CodeArgs FindArgs
        {
            get
            {
                if(HasExts)
                    return Exts;
                if(HasCode)
                    return Code.Exts;
                return null;
            }
        }

        [DisableDump]
        internal CodeArgs SmartArgs
        {
            get
            {
                var result = FindArgs;
                if(result == null)
                {
                    DumpMethodWithBreak("No approriate result property defined");
                    Debugger.Break();
                }
                return result;
            }
        }

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
                if(HasHllw && Hllw == false)
                    return false;
                if(HasSize && !Size.IsZero)
                    return false;
                if(HasType && !(Type is VoidType))
                    return false;
                if(HasCode && !Code.IsEmpty)
                    return false;
                if(HasExts && !Exts.IsNone)
                    return false;
                return true;
            }
        }

        internal bool HasArg
        {
            get
            {
                if(HasExts)
                    return Exts.HasArg;
                if(HasCode)
                    return Code.HasArg;
                return false;
            }
        }
        public Category PendingCategory
        {
            get { return _pendingCategory; }
            set
            {
                _pendingCategory = value;
                AssertValid();
            }
        }

        Result IAggregateable<Result>.Aggregate(Result other) { return this + other; }

        public override string DumpData()
        {
            var result = "";
            if(_pendingCategory != Category.None)
                result += "\nPendingCategory=" + _pendingCategory.Dump();
            if(CompleteCategory != Category.None)
                result += "\nCompleteCategory=" + CompleteCategory.Dump();
            if(HasHllw)
                result += "\nHllw=" + Tracer.Dump(_hllw);
            if(HasSize)
                result += "\nSize=" + Tracer.Dump(_size);
            if(HasType)
                result += "\nType=" + Tracer.Dump(_type);
            if(HasExts)
                result += "\nExts=" + Tracer.Dump(_exts);
            if(HasCode)
                result += "\nCode=" + Tracer.Dump(_code);
            if(result == "")
                return "";
            return result.Substring(1);
        }

        internal void Update(Result result)
        {
            if(result.HasHllw)
                _hllw = result.Hllw;

            if(result.HasSize)
                _size = result.Size;

            if(result.HasType)
                _type = result.Type;

            if(result.HasExts)
                _exts = result.Exts;

            if(result.HasCode)
                _code = result.Code;

            _pendingCategory = _pendingCategory - result.CompleteCategory;

            AssertValid();
        }

        Result Filter(Category category)
        {
            return new Result(CompleteCategory & category, () => Hllw.Value, () => Size, () => Type, () => Code, () => Exts)
            {
                _pendingCategory = _pendingCategory & category
            };
        }

        [DisableDump]
        internal Result Align
        {
            get
            {
                var size = FindSize;
                if(size == null)
                    return this;

                var alignBits = Root.DefaultRefAlignParam.AlignBits;
                var alignedSize = size.Align(alignBits);
                if(alignedSize == size)
                    return this;

                var result = new Result
                    (
                    CompleteCategory,
                    () => Hllw.Value,
                    () => alignedSize,
                    () => Type.UniqueAlign,
                    () => Code.BitCast(alignedSize),
                    () => Exts);
                return result;
            }
        }

        [DisableDump]
        internal Result Clone { get { return Filter(CompleteCategory); } }

        void AssertValid()
        {
            if(IsDirty)
                return;

            if(HasHllw && HasSize)
                Tracer.Assert
                    (Size.IsZero == Hllw, () => "Size and Hllw differ: " + Dump());
            if(HasHllw && HasType && Type.HasQuickSize)
                Tracer.Assert
                    (Type.Hllw == Hllw, () => "Type and Hllw differ: " + Dump());
            if(HasHllw && HasCode)
                Tracer.Assert
                    (Code.Hllw == Hllw, () => "Code and Hllw differ: " + Dump());
            if(HasSize && HasType && Type.HasQuickSize)
                Tracer.Assert(Type.Size == Size, () => "Type and Size differ: " + Dump());
            if(HasSize && HasCode)
                Tracer.Assert(Code.Size == Size, () => "Code and Size differ: " + Dump());
            if(HasType && HasCode && Type.HasQuickSize)
                Tracer.Assert(Code.Size == Type.Size, () => "Code and Type differ: " + Dump());
            if(HasExts && HasCode)
                Tracer.Assert
                    (Code.Exts.IsEqual(Exts), () => "Code and Exts differ: " + Dump());

            Tracer.Assert((CompleteCategory & PendingCategory) == Category.None);
        }

        void Add(Result other) { Add(other, CompleteCategory); }

        void Add(Result other, Category category)
        {
            Tracer.Assert(category <= other.CompleteCategory);
            Tracer.Assert(category <= CompleteCategory);
            IsDirty = true;
            if(category.HasHllw)
                Hllw = SmartHllw && other.SmartHllw;
            if(category.HasSize)
                Size += other.Size;
            if(category.HasType)
                Type = Type.Pair(other.Type);
            if(category.HasCode)
                Code = Code + other.Code;
            if(category.HasExts)
                Exts = Exts.Sequence(other.Exts);
            IsDirty = false;
        }

        Result Sequence(Result second)
        {
            var result = Clone;
            result.Add(second);
            return result;
        }

        internal Result ReplaceArg(Func<Category, Result> getResultForArg)
        {
            if(HasArg && getResultForArg != null)
                return InternalReplaceArg(getResultForArg);
            return this;
        }

        internal Result ReplaceArg(ResultCache resultCache)
        {
            if(HasArg)
                return InternalReplaceArg(category => resultCache & category);
            return this;
        }

        internal Result ReplaceArg(Result resultForArg)
        {
            if(HasArg && resultForArg != null)
                return InternalReplaceArg(category => resultForArg);
            return this;
        }

        Result InternalReplaceArg(Func<Category, Result> getResultForArg)
        {
            var result = new Result
            {
                Hllw = Hllw,
                Size = Size,
                Type = Type,
                Code = Code,
                Exts = Exts,
                IsDirty = true
            };

            var categoryForArg = (CompleteCategory & Category.Code.CodeExtsed);
            if(HasCode)
                categoryForArg |= Category.Type;

            var resultForArg = getResultForArg(categoryForArg);
            if(resultForArg != null)
            {
                if(HasCode)
                    result.Code = Code.ReplaceArg(getResultForArg(Category.Type | Category.Code));
                if(HasExts)
                    result.Exts = Exts.WithoutArg()
                        + getResultForArg(Category.Exts).Exts;
            }
            result.IsDirty = false;
            return result;
        }

        internal Result ReplaceAbsolute<TRefInCode>
            (TRefInCode refInCode, Func<CodeBase> replacementCode, Func<CodeArgs> replacementRefs)
            where TRefInCode : IContextReference
        {
            if(HasExts && !Exts.Contains(refInCode))
                return this;

            var result = new Result
            {
                Hllw = Hllw,
                Size = Size,
                Type = Type,
                IsDirty = true
            };
            if(HasCode)
                result.Code = Code.ReplaceAbsolute(refInCode, replacementCode);
            if(HasExts)
                result.Exts = Exts.Without(refInCode).Sequence(replacementRefs());
            result.IsDirty = false;
            return result;
        }

        internal Result ReplaceAbsolute<TRefInCode>
            (TRefInCode refInCode, Func<Category, Result> getReplacement)
            where TRefInCode : IContextReference
        {
            if(HasExts && !Exts.Contains(refInCode))
                return this;

            var replacement = getReplacement(CompleteCategory - Category.Size - Category.Type);
            var result = new Result
            {
                Hllw = Hllw,
                Size = Size,
                Type = Type,
                IsDirty = true
            };
            if(HasCode)
                result.Code = Code.ReplaceAbsolute(refInCode, () => replacement.Code);
            if(HasExts)
                result.Exts = Exts.Without(refInCode).Sequence(replacement.Exts);
            result.IsDirty = false;
            return result;
        }

        internal Result ReplaceRelative<TRefInCode>
            (TRefInCode refInCode, Func<CodeBase> replacementCode, Func<CodeArgs> replacementRefs)
            where TRefInCode : IContextReference
        {
            if(HasExts && !Exts.Contains(refInCode))
                return this;

            var result = new Result {Hllw = Hllw, Size = Size, Type = Type};
            if(HasCode)
                result.Code = Code.ReplaceRelative(refInCode, replacementCode);
            if(HasExts)
                result.Exts = Exts.Without(refInCode).Sequence(replacementRefs());
            return result;
        }

        internal Result ReplaceRefsForFunctionBody(Size refSize, CodeBase replacement)
        {
            if(!HasCode)
                return this;
            if(SmartArgs.Count == 0)
                return this;
            var result = Clone;
            result.IsDirty = true;
            result.Code = SmartArgs.ReplaceRefsForFunctionBody(Code, refSize, replacement);
            result.Exts = replacement.Exts;
            result.IsDirty = false;
            return result;
        }

        internal Result LocalBlock(Category category)
        {
            if(!category.HasCode && !category.HasExts)
                return this;

            var result = this & category;
            var copier = Type.Copier(category);
            if(category.HasCode)
                result.Code = Code.LocalBlock(copier.Code);
            if(category.HasExts)
                result.Exts = Exts.Sequence(copier.Exts);
            return result;
        }

        internal Result Conversion(TypeBase target)
        {
            var conversion = Type.Conversion(CompleteCategory, target);
            return conversion.ReplaceArg(this);
        }

        internal BitsConst Evaluate(IExecutionContext context)
        {
            Tracer.Assert(Exts.IsNone, Dump);
            var result = Align.LocalBlock(CompleteCategory);
            return result.Code.Evaluate(context);
        }

        [DisableDump]
        internal Result AutomaticDereferenceResult
        {
            get
            {
                Tracer.Assert(HasType, () => "Dereference requires type category:\n " + Dump());

                var result = this;
                while(result.Type.IsWeakReference)
                    result = result.DereferenceResult;
                return result;
            }
        }

        [DisableDump]
        internal Result DereferenceResult
        {
            get
            {
                Tracer.Assert(HasType, () => "Dereference requires type category:\n " + Dump());
                var referenceType = Type.ReferenceType;
                var converter = referenceType.Converter;
                var result = converter.Result(CompleteCategory);
                return result
                    .ReplaceArg(this);
            }
        }

        [DisableDump]
        internal Result UnalignedResult
        {
            get
            {
                Tracer.Assert(HasType, () => "UnalignedResult requires type category:\n " + Dump());
                return ((AlignType) Type)
                    .UnalignedResult(CompleteCategory)
                    .ReplaceArg(this);
            }
        }

        [DebuggerHidden]
        public static Result operator &(Result result, Category category)
        {
            return result.Filter(category);
        }

        [DebuggerHidden]
        public static Result operator |(Result aResult, Result bResult)
        {
            Tracer.Assert((aResult.CompleteCategory & bResult.CompleteCategory).IsNone);
            var result = aResult.Clone;
            result.Update(bResult);
            return result;
        }

        [DebuggerHidden]
        public static Result operator +(Result aResult, Result bResult)
        {
            return aResult.Sequence(bResult);
        }

        [DisableDump]
        internal Result LocalPointerKindResult
        {
            get
            {
                if(Type.Hllw)
                    return this;
                if(Type is IReferenceType)
                    return this;
                return Type
                    .LocalReferenceResult(CompleteCategory)
                    .ReplaceArg(this);
            }
        }

        internal Result ContextReferenceViaStructReference(Structure accessPoint)
        {
            return accessPoint
                .ContextReferenceViaStructReference(this);
        }

        [DebuggerHidden]
        internal void AssertVoidOrValidReference()
        {
            var size = FindSize;
            if(size != null)
                Tracer.Assert
                    (
                        size.IsZero || size == Root.DefaultRefAlignParam.RefSize,
                        () => "Expected size: 0 or RefSize\n" + Dump());

            if(HasType)
                Tracer.Assert
                    (
                        Type is VoidType || Type is PointerType,
                        () => "Expected type: Void or PointerType\n" + Dump());
        }

        [DebuggerHidden]
        internal void AssertValidReference()
        {
            var size = FindSize;
            if(size != null)
                Tracer.Assert
                    (
                        size == Root.DefaultRefAlignParam.RefSize,
                        () => "Expected size: RefSize\n" + Dump());

            if(HasType)
                Tracer.Assert(Type is PointerType, () => "Expected type: PointerType\n" + Dump());
        }

        [DebuggerHidden]
        internal void AssertEmptyOrValidReference()
        {
            if(FindHllw == true)
                return;

            var size = FindSize;
            if(size != null)
            {
                if(size.IsZero)
                    return;
                Tracer.Assert
                    (
                        size == Root.DefaultRefAlignParam.RefSize,
                        () => "Expected size: 0 or RefSize\n" + Dump());
            }

            if(HasType)
                Tracer.Assert(Type is PointerType, () => "Expected type: PointerType\n" + Dump());
        }

        internal Result AddToReference(Func<Size> func) { return Change(code => code.ReferencePlus(func())); }

        public Result Change(Func<CodeBase, CodeBase> func)
        {
            if(!HasCode)
                return this;
            var result = Clone;
            result.Code = func(Code);
            return result;
        }

        internal Result Un<T>()
            where T : IConverter
        {
            var result = ((IConverter) Type).Result(CompleteCategory);
            return result.ReplaceArg(this);
        }

        internal Result SmartUn<T>() where T : IConverter { return Type is T ? Un<T>() : this; }

        internal Result BitSequenceOperandConversion(Category category)
        {
            return Type.BitSequenceOperandConversion(category).ReplaceArg(this);
        }

        internal Result DereferencedAlignedResult()
        {
            var destinationType = Type
                .AutomaticDereferenceType.UniqueAlign;
            return Type
                .ObviousExactConversion(CompleteCategory, destinationType)
                .ReplaceArg(this);
        }

        internal Result ObviousExactConversion(TypeBase destinationType)
        {
            return Type
                .ObviousExactConversion(CompleteCategory, destinationType)
                .ReplaceArg(this);
        }
    }
}