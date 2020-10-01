using System;
using System.Diagnostics;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Type;
using Reni.Validation;

namespace Reni
{
    sealed class Result : DumpableObject, IAggregateable<Result>
    {
        static int _nextObjectId = 1;
        CodeBase _code;
        CodeArgs _exts;
        /// <summary>
        /// Is this an hollow object? With no data?
        /// </summary>
        bool? _isHollow;
        bool _isDirty;
        Category _pendingCategory;
        Size _size;
        TypeBase _type;

        public Issue[] Issues = new Issue[0];

        public Result()
            : base(_nextObjectId++) => _pendingCategory = Category.None;

        internal Result(Category category, Issue[] issues)
            : this
            (
                category & Category.Exts,
                issues,
                getIsHollow: null,
                getSize: null,
                getType: null,
                getCode: null,
                getExts: CodeArgs.Void)
        {
        }

        internal Result(Category category, Issue recentIssue)
            : this(category, new[] {recentIssue})
        {
        }

        internal Result
        (
            Category category,
            Func<bool> getIsHollow = null,
            Func<Size> getSize = null,
            Func<TypeBase> getType = null,
            Func<CodeBase> getCode = null,
            Func<CodeArgs> getExts = null,
            Root rootContext = null)
            : this
            (
                category,
                issues: null,
                getIsHollow: getIsHollow,
                getSize: getSize,
                getType: getType,
                getCode: getCode,
                getExts: getExts,
                rootContext: rootContext)
        {
        }

        Result
        (
            Category category,
            Issue[] issues,
            Func<bool> getIsHollow,
            Func<Size> getSize,
            Func<TypeBase> getType,
            Func<CodeBase> getCode,
            Func<CodeArgs> getExts,
            Root rootContext = null)
            : this()
        {
            Issues = issues ?? new Issue[0];

            var isHollow = getIsHollow == null ? null : new ValueCache<bool>(getIsHollow);
            var size = getSize == null ? null : new ValueCache<Size>(getSize);
            var type = getType == null ? null : new ValueCache<TypeBase>(getType);
            var code = getCode == null ? null : new ValueCache<CodeBase>(getCode);
            var exts = getExts == null ? null : new ValueCache<CodeArgs>(getExts);

            if(category.HasType)
                _type = ObtainType(isHollow, size, type, code, rootContext);

            if(category.HasCode)
                _code = ObtainCode(isHollow, size, type, code);

            if(category.HasSize)
                _size = ObtainSize(isHollow, size, type, code);

            if(category.HasExts)
                _exts = ObtainExts(isHollow, size, type, code, exts);

            if(category.HasIsHollow)
                _isHollow = ObtainIsHollow(isHollow, size, type, code);

            AssertValid();
        }

        Result IAggregateable<Result>.Aggregate(Result other) => this + other;

        internal bool HasSize => Size != null;
        internal bool HasType => Type != null;
        internal bool HasCode => Code != null;
        internal bool HasExts => Exts != null;
        internal bool HasIsHollow => _isHollow != null;

        [Node]
        [EnableDumpWithExceptionPredicate]
        public Category CompleteCategory
            => Category.CreateCategory(HasIsHollow, HasSize, HasType, HasCode, HasExts);

        /// <summary>
        /// Is this an hollow object? With no data?
        /// </summary>
        [Node]
        [DebuggerHidden]
        public bool? IsHollow
        {
            get => _isHollow;
            set
            {
                _isHollow = value;
                AssertValid();
            }
        }

        [Node]
        [DebuggerHidden]
        public Size Size
        {
            get => _size;
            set
            {
                _size = value;
                AssertValid();
            }
        }

        [Node]
        [DebuggerHidden]
        internal TypeBase Type
        {
            get => _type;
            set
            {
                _type = value;
                AssertValid();
            }
        }

        [Node]
        internal CodeBase Code
        {
            [DebuggerHidden]
            get { return _code; }
            [DebuggerHidden]
            set
            {
                _code = value;
                AssertValid();
            }
        }

        [Node]
        [DebuggerHidden]
        internal CodeArgs Exts
        {
            get => _exts;
            set
            {
                _exts = value;
                AssertValid();
            }
        }

        internal bool? FindIsHollow => HasIsHollow ? _isHollow : FindSize?.IsZero;

        bool? QuickFindIsHollow => HasIsHollow ? _isHollow : QuickFindSize?.IsZero;

        internal bool SmartIsHollow
        {
            get
            {
                var result = FindIsHollow;
                if(result == null)
                {
                    DumpMethodWithBreak(text: "No approriate result property defined");
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
                    DumpMethodWithBreak(text: "No approriate result property defined");
                    Debugger.Break();
                }
                return result;
            }
        }

        internal CodeArgs FindExts
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
        internal CodeArgs SmartExts
        {
            get
            {
                var result = FindExts;
                if(result == null)
                {
                    DumpMethodWithBreak(text: "No approriate result property defined");
                    Debugger.Break();
                }
                return result;
            }
        }

        internal bool IsDirty
        {
            get => _isDirty;
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
                if(HasIsHollow && IsHollow == false)
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
            get => _pendingCategory;
            set
            {
                _pendingCategory = value;
                AssertValid();
            }
        }

        [DisableDump]
        internal Result Align
        {
            get
            {
                if(HasIssue)
                    return this;

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
                    () => IsHollow.Value,
                    () => alignedSize,
                    () => Type.Align,
                    () => Code.BitCast(alignedSize),
                    () => Exts);
                return result;
            }
        }

        public bool HasIssue => Issues.Any();

        [DisableDump]
        internal Result Weaken
        {
            get
            {
                var weakType = Type?.Weaken;
                return weakType == null
                    ? this
                    : (Type.Mutation(weakType) & CompleteCategory).ReplaceArg(this);
            }
        }

        [DisableDump]
        internal Result Clone => Filter(CompleteCategory);

        [DisableDump]
        internal Result AutomaticDereferenceResult
        {
            get
            {
                if(HasIssue)
                    return this;

                Tracer.Assert(HasType, () => "Dereference requires type category:\n " + Dump());

                var result = this;
                while(!result.HasIssue && result.Type.IsWeakReference)
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
                var referenceType = Type.CheckedReference;
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

        [DisableDump]
        internal Result LocalReferenceResult
        {
            get
            {
                if(HasIssue)
                    return this;
                if(!HasCode && !HasType && !HasSize)
                    return this;
                if(Type.IsHollow)
                    return this;
                if(Type is IReference)
                    return this;
                return Type
                    .LocalReferenceResult(CompleteCategory)
                    .ReplaceArg(this);
            }
        }

        CodeBase ObtainCode
        (
            ValueCache<bool> getIsHollow,
            ValueCache<Size> getSize,
            ValueCache<TypeBase> getType,
            ValueCache<CodeBase> getCode)
        {
            if(getCode != null)
                return getCode.Value;
// ReSharper disable ExpressionIsAlwaysNull
            var isHollow = TryObtainIsHollow(getIsHollow, getSize, getType, getCode);
// ReSharper restore ExpressionIsAlwaysNull
            if(isHollow == true)
                return CodeBase.Void;
            Tracer.AssertionFailed(cond: "Code cannot be determined", getText: ToString);
            return null;
        }

        TypeBase ObtainType
        (
            ValueCache<bool> getIsHollow,
            ValueCache<Size> getSize,
            ValueCache<TypeBase> getType,
            ValueCache<CodeBase> getCode,
            Root rootContext)
        {
            if(getType != null)
                return getType.Value;
// ReSharper disable ExpressionIsAlwaysNull
            var isHollow = TryObtainIsHollow(getIsHollow, getSize, getType, getCode);
// ReSharper restore ExpressionIsAlwaysNull
            if(isHollow == true)
                return rootContext.VoidType;
            Tracer.AssertionFailed(cond: "Type cannot be determned", getText: ToString);
            return null;
        }

        Size ObtainSize
        (
            ValueCache<bool> getIsHollow,
            ValueCache<Size> getSize,
            ValueCache<TypeBase> getType,
            ValueCache<CodeBase> getCode)
        {
            var result = TryObtainSize(getIsHollow, getSize, getType, getCode);
            Tracer.Assert(result != null, () => "Size cannot be determned " + ToString());
            return result;
        }

        static Size TryObtainSize
        (
            ValueCache<bool> getIsHollow,
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
            if(getIsHollow != null && getIsHollow.Value)
                return Size.Zero;
            return null;
        }

        bool ObtainIsHollow
        (
            ValueCache<bool> getIsHollow,
            ValueCache<Size> getSize,
            ValueCache<TypeBase> getType,
            ValueCache<CodeBase> getCode)
        {
            var result = TryObtainIsHollow(getIsHollow, getSize, getType, getCode);
            if(result != null)
                return result.Value;
            Tracer.AssertionFailed(cond: "Datalessness cannot be determned", getText: ToString);
            return false;
        }

        static bool? TryObtainIsHollow
        (
            ValueCache<bool> getIsHollow,
            ValueCache<Size> getSize,
            ValueCache<TypeBase> getType,
            ValueCache<CodeBase> getCode) =>
            getIsHollow?.Value ?? getSize?.Value.IsZero ?? getType?.Value.IsHollow ?? getCode?.Value.IsEmpty;

        CodeArgs ObtainExts
        (
            ValueCache<bool> getIsHollow,
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
            if(TryObtainIsHollow(getIsHollow, getSize, getType, getCode) == true)
// ReSharper restore ExpressionIsAlwaysNull
                return CodeArgs.Void();

            Tracer.AssertionFailed(cond: "CodeArgs cannot be determned", getText: ToString);
            return null;
        }

        public override string DumpData()
        {
            var result = "";
            if(_pendingCategory != Category.None)
                result += "\nPendingCategory=" + _pendingCategory.Dump();
            if(CompleteCategory != Category.None)
                result += "\nCompleteCategory=" + CompleteCategory.Dump();
            if (HasIssue)
                result += "\nIssues=" + Tracer.Dump(Issues);
            if (HasIsHollow)
                result += "\nIsHollow=" + Tracer.Dump(_isHollow);
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
            return result.Substring(startIndex: 1);
        }

        internal void Update(Result result)
        {
            Issues = Issues.Union(result.Issues).ToArray();

            if(HasIssue)
                Reset(Category.All - Category.Exts);
            else
            {
                if(result.HasIsHollow)
                    _isHollow = result.IsHollow;

                if(result.HasSize)
                    _size = result.Size;

                if(result.HasType)
                    _type = result.Type;

                if(result.HasCode)
                    _code = result.Code;
            }

            if(result.HasExts)
                _exts = result.Exts;

            _pendingCategory = _pendingCategory - result.CompleteCategory;

            AssertValid();
        }

        Result Filter(Category category)
        {
            return new Result
            (
                CompleteCategory & category,
                Issues,
                () => IsHollow.Value,
                () => Size,
                () => Type,
                () => Code,
                () => Exts)
            {
                _pendingCategory = _pendingCategory & category
            };
        }

        void AssertValid()
        {
            if(IsDirty)
                return;

            Tracer.Assert((CompleteCategory & PendingCategory) == Category.None);

            if(HasIssue)
            {
                Tracer.Assert(!HasIsHollow);
                Tracer.Assert(!HasSize);
                Tracer.Assert(!HasType);
                Tracer.Assert(!HasCode);
                return;
            }

            if(HasIsHollow && HasSize)
                Tracer.Assert
                    (Size.IsZero == IsHollow, () => "Size and IsHollow differ: " + Dump());
            if(HasIsHollow && HasType && Type.HasQuickSize)
                Tracer.Assert
                    (Type.IsHollow == IsHollow, () => "Type and IsHollow differ: " + Dump());
            if(HasIsHollow && HasCode)
                Tracer.Assert
                    (Code.IsHollow == IsHollow, () => "Code and IsHollow differ: " + Dump());
            if(HasSize && HasType && Type.HasQuickSize)
                Tracer.Assert(Type.Size == Size, () => "Type and Size differ: " + Dump());
            if(HasSize && HasCode)
                Tracer.Assert(Code.Size == Size, () => "Code and Size differ: " + Dump());
            if(HasType && HasCode && Type.HasQuickSize)
                Tracer.Assert(Code.Size == Type.Size, () => "Code and Type differ: " + Dump());
            if(HasExts && HasCode)
                Tracer.Assert
                    (Code.Exts.IsEqual(Exts), () => "Code and Exts differ: " + Dump());
        }

        void Add(Result other, SourcePart position) => Add(other, CompleteCategory, position);

        void Add(Result other, Category category, SourcePart position)
        {
            IsDirty = true;

            Issues = Issues.Union(other.Issues).ToArray();

            var hasIssue = HasIssue;

            if(hasIssue)
            {
                IsHollow = null;
                Size = null;
                Type = null;
                Code = null;
            }
            else
            {
                Tracer.Assert
                (
                    category <= other.CompleteCategory,
                    () => nameof(other).DumpValue(other) + ", " + nameof(category).DumpValue(category)
                );

                Tracer.Assert
                (
                    category <= CompleteCategory,
                    () => "this".DumpValue(this) + ", " + nameof(category).DumpValue(category)
                );

                if(category.HasIsHollow)
                    IsHollow = SmartIsHollow && other.SmartIsHollow;
                else if(HasIsHollow)
                    IsHollow = null;

                if(category.HasSize)
                    Size += other.Size;
                else if(HasSize)
                    Size = null;

                if(category.HasType)
                    Type = Type.Pair(other.Type, position);
                else if(HasType)
                    Type = null;

                if(category.HasCode)
                    Code = Code + other.Code;
                else if(HasCode)
                    Code = null;
            }

            if(category.HasExts)
                Exts = Exts.Sequence(other.Exts);
            else if(HasExts)
                Exts = null;

            IsDirty = false;
        }

        internal void Reset(Category category)
        {
            IsDirty = true;
            if(category.HasIsHollow)
                IsHollow = null;
            if(category.HasSize)
                Size = null;
            if(category.HasType)
                Type = null;
            if(category.HasCode)
                Code = null;
            if(category.HasExts)
                Exts = null;
            IsDirty = false;
        }

        internal Result Sequence(Result second, SourcePart position)
        {
            var result = Clone;
            result.Add(second, position);
            return result;
        }

        internal Result ReplaceArg(ResultCache resultCache)
            => HasArg ? InternalReplaceArg(resultCache) : this;

        internal Result ReplaceArg(ResultCache.IResultProvider provider)
            => HasArg ? InternalReplaceArg(new ResultCache(provider)) : this;

        internal Result ReplaceArg(Func<Category, Result> getArgs)
            => HasArg ? InternalReplaceArg(new ResultCache(getArgs)) : this;

        Result InternalReplaceArg(ResultCache getResultForArg)
        {
            var result = new Result
            {
                Issues = Issues,
                IsHollow = IsHollow,
                Size = Size,
                Type = Type,
                Code = Code,
                Exts = Exts,
                IsDirty = true
            };

            var categoryForArg = CompleteCategory & Category.Code.CodeExtsed;
            if(HasCode)
                categoryForArg |= Category.Type;

            var resultForArg = getResultForArg & categoryForArg;
            if(resultForArg != null)
            {
                result.Issues = result.Issues.Union(resultForArg.Issues).ToArray();

                if (HasCode)
                    result.Code = Code.ReplaceArg(resultForArg);
                if(HasExts)
                    result.Exts = Exts.WithoutArg() + resultForArg.Exts;

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

            if(!HasCode && !HasExts)
                return this;

            var result = new Result
            {
                Issues = Issues,
                IsHollow = IsHollow,
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

            if(!HasCode && !HasExts)
                return this;

            var replacement = getReplacement(CompleteCategory - Category.Size - Category.Type);
            var result = new Result
            {
                Issues = Issues,
                IsHollow = IsHollow,
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

            if(!HasCode && !HasExts)
                return this;

            var result = new Result
            {
                Issues = Issues,
                IsHollow = IsHollow,
                Size = Size,
                Type = Type
            };
            if(HasCode)
                result.Code = Code.ReplaceRelative(refInCode, replacementCode);
            if(HasExts)
                result.Exts = Exts.Without(refInCode).Sequence(replacementRefs());
            return result;
        }

        internal Result ReplaceRefsForFunctionBody(CodeBase replacement)
        {
            if(!HasCode)
                return this;
            if(SmartExts.Count == 0)
                return this;
            var result = Clone;
            result.IsDirty = true;
            result.Code = SmartExts.ReplaceRefsForFunctionBody(Code, replacement);
            result.Exts = replacement.Exts;
            result.IsDirty = false;
            return result;
        }

        internal Result LocalBlock(Category category)
            => AutomaticDereferenceResult.InternalLocalBlock(category.Typed);

        internal Result InternalLocalBlock(Category category)
        {
            if(HasIssue)
                return this;

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

        [DebuggerHidden]
        public static Result operator &(Result result, Category category) => result.Filter(category);

        [DebuggerHidden]
        public static Result operator |(Result aResult, Result bResult)
        {
            Tracer.Assert((aResult.CompleteCategory & bResult.CompleteCategory).IsNone);
            var result = aResult.Clone;
            result.Update(bResult);
            return result;
        }

        public static Result operator +(Result aResult, Result bResult) => aResult.Sequence(bResult, position: null);

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
            if(FindIsHollow == true)
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

        Result Change(Func<CodeBase, CodeBase> func)
        {
            if(!HasCode)
                return this;
            var result = Clone;
            result.Code = func(Code);
            return result;
        }

        Result Un()
        {
            var result = ((IConversion) Type).Result(CompleteCategory);
            return result.ReplaceArg(this);
        }

        internal Result SmartUn<T>()
            where T : IConversion => Type is T ? Un() : this;

        internal Result AutomaticDereferencedAlignedResult()
        {
            var destinationType = Type
                .AutomaticDereferenceType.Align;

            if(destinationType == Type)
                return this;

            return Type
                .Conversion(CompleteCategory, destinationType)
                .ReplaceArg(this);
        }

        internal Result DereferencedAlignedResult(Size size)
            => HasIssue
                ? this
                : HasCode
                    ? new Result(CompleteCategory - Category.Type, getCode: () => Code.DePointer(size))
                    : this;

        internal Result ConvertToConverter(TypeBase source)
            => source.IsHollow || !HasExts && !HasCode
                ? this
                : ReplaceAbsolute(source.CheckedReference, source.ArgResult);

        internal Result AddCleanup(Result cleanup)
        {
            if(!HasCode || !HasExts || cleanup.IsEmpty)
                return this;

            if(HasExts && !cleanup.Exts.IsNone)
                NotImplementedMethod(cleanup);

            var result = Clone;
            if(HasCode)
                result.Code = result.Code.AddCleanup(cleanup.Code);
            return result;
        }

        internal Result ArrangeCleanupCode()
        {
            if(!HasCode)
                return this;

            var newCode = Code.ArrangeCleanupCode();
            if(newCode == null)
                return this;

            var result = Clone;
            result.Code = newCode;
            return result;
        }

        internal Result InvalidConversion(TypeBase destination)
            =>
                destination.Result
                    (CompleteCategory, () => Code.InvalidConversion(destination.Size), () => Exts);

        internal bool IsValidOrIssue(Category category)
            => HasIssue || category <= CompleteCategory;
    }
}