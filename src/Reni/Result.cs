using System;
using System.Diagnostics;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using JetBrains.Annotations;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Type;
using Reni.Validation;

namespace Reni;

sealed class Result : DumpableObject, IAggregateable<Result>
{
    sealed class DataContainer
    {
        internal Closures Closure;
        internal CodeBase Code;
        internal bool IsDirty;
        internal bool? IsHollow;
        internal Category PendingCategory;
        internal Size Size;
        internal TypeBase Type;
    }

    static int NextObjectId = 1;

    /// <summary>
    ///     Is this an hollow object? With no data?
    /// </summary>
    public Issue[] Issues = new Issue[0];

    readonly DataContainer Data = new();

    internal bool HasSize => Size != null;
    internal bool HasType => Type != null;
    internal bool HasCode => Code != null;
    internal bool HasClosures => Closures != null;
    internal bool HasIsHollow => Data.IsHollow != null;

    [Node]
    [EnableDumpWithExceptionPredicate]
    public Category CompleteCategory
        => Category.CreateCategory(HasIsHollow, HasSize, HasType, HasCode, HasClosures);

    /// <summary>
    ///     Is this an hollow object? With no data?
    /// </summary>
    [Node]
    [DebuggerHidden]
    public bool? IsHollow
    {
        get => Data.IsHollow;
        set
        {
            Data.IsHollow = value;
            AssertValid();
        }
    }

    [Node]
    [DebuggerHidden]
    public Size Size
    {
        get => Data.Size;
        set
        {
            Data.Size = value;
            AssertValid();
        }
    }

    [Node]
    [DebuggerHidden]
    internal TypeBase Type
    {
        get => Data.Type;
        set
        {
            Data.Type = value;
            AssertValid();
        }
    }

    [Node]
    internal CodeBase Code
    {
        [DebuggerHidden]
        get => Data.Code;
        [DebuggerHidden]
        set
        {
            Data.Code = value;
            AssertValid();
        }
    }

    [Node]
    [DebuggerHidden]
    internal Closures Closures
    {
        get => Data.Closure;
        set
        {
            Data.Closure = value;
            AssertValid();
        }
    }

    internal bool? FindIsHollow => HasIsHollow? Data.IsHollow : FindSize?.IsZero;

    [PublicAPI]
    bool? QuickFindIsHollow => HasIsHollow? Data.IsHollow : QuickFindSize?.IsZero;

    internal bool SmartIsHollow
    {
        get
        {
            var result = FindIsHollow;
            if(result == null)
            {
                DumpMethodWithBreak("No appropriate result property defined");
                Debugger.Break();
                return false;
            }

            return result.Value;
        }
    }

    internal Size FindSize
        => HasSize
            ? Size
            : HasCode
                ? Code.Size
                : HasType
                    ? Type.Size
                    : null;

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
                DumpMethodWithBreak("No appropriate result property defined");
                Debugger.Break();
            }

            return result;
        }
    }

    internal Closures FindClosures
        => HasClosures
            ? Closures
            : HasCode
                ? Code.Closures
                : null;

    [DisableDump]
    internal Closures SmartClosures
    {
        get
        {
            var result = FindClosures;
            if(result == null)
            {
                DumpMethodWithBreak("No appropriate result property defined");
                Debugger.Break();
            }

            return result;
        }
    }

    internal bool IsDirty
    {
        get => Data.IsDirty;
        set
        {
            Data.IsDirty = value;
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
            if(HasClosures && !Closures.IsNone)
                return false;
            return true;
        }
    }

    internal bool HasArg
        => HasClosures? Closures.HasArg : HasCode && Code.HasArg;

    public Category PendingCategory
    {
        get => Data.PendingCategory;
        set
        {
            Data.PendingCategory = value;
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
                () => IsHollow.AssertValue(),
                () => alignedSize,
                () => Type.Align,
                () => Code.BitCast(alignedSize),
                () => Closures);
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

            HasType.Assert(() => "Dereference requires type category:\n " + Dump());

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
            HasType.Assert(() => "Dereference requires type category:\n " + Dump());
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
            HasType.Assert(() => "UnalignedResult requires type category:\n " + Dump());
            return ((AlignType)Type)
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

    public Result()
        : base(NextObjectId++) => Data.PendingCategory = Category.None;

    internal Result(Category category, Issue[] issues)
        : this
        (
            category & Category.Closures,
            issues,
            null,
            null,
            null,
            null,
            Closures.Void) { }

    internal Result(Category category, Issue recentIssue)
        : this(category, new[] { recentIssue }) { }

    internal Result
    (
        Category category,
        Func<bool> getIsHollow = null,
        Func<Size> getSize = null,
        Func<TypeBase> getType = null,
        Func<CodeBase> getCode = null,
        Func<Closures> getClosures = null,
        Root rootContext = null
    )
        : this
        (
            category,
            null,
            getIsHollow,
            getSize,
            getType,
            getCode,
            getClosures,
            rootContext) { }

    Result
    (
        Category category,
        Issue[] issues,
        Func<bool> getIsHollow,
        Func<Size> getSize,
        Func<TypeBase> getType,
        Func<CodeBase> getCode,
        Func<Closures> getClosures,
        Root rootContext = null
    )
        : this()
    {
        Issues = issues ?? new Issue[0];

        var isHollow = getIsHollow == null? null : new ValueCache<bool>(getIsHollow);
        var size = getSize == null? null : new ValueCache<Size>(getSize);
        var type = getType == null? null : new ValueCache<TypeBase>(getType);
        var code = getCode == null? null : new ValueCache<CodeBase>(getCode);
        var closures = getClosures == null? null : new ValueCache<Closures>(getClosures);

        if(category.HasType)
            Data.Type = ObtainType(isHollow, size, type, code, rootContext);

        if(category.HasCode)
            Data.Code = ObtainCode(isHollow, size, type, code);

        if(category.HasSize)
            Data.Size = ObtainSize(isHollow, size, type, code);

        if(category.HasClosures)
            Data.Closure = ObtainClosures(isHollow, size, type, code, closures);

        if(category.HasIsHollow)
            Data.IsHollow = ObtainIsHollow(isHollow, size, type, code);

        AssertValid();
    }

    Result IAggregateable<Result>.Aggregate(Result other) => this + other;

    public override string DumpData()
    {
        var result = "";
        if(Data.PendingCategory != Category.None)
            result += "\nPendingCategory=" + Data.PendingCategory.Dump();
        if(CompleteCategory != Category.None)
            result += "\nCompleteCategory=" + CompleteCategory.Dump();
        if(HasIssue)
            result += "\nIssues=" + Tracer.Dump(Issues);
        if(HasIsHollow)
            result += "\nIsHollow=" + Tracer.Dump(Data.IsHollow);
        if(HasSize)
            result += "\nSize=" + Tracer.Dump(Data.Size);
        if(HasType)
            result += "\nType=" + Tracer.Dump(Data.Type);
        if(HasClosures)
            result += "\nClosures=" + Tracer.Dump(Data.Closure);
        if(HasCode)
            result += "\nCode=" + Tracer.Dump(Data.Code);
        if(result == "")
            return "";
        return result.Substring(1);
    }

    CodeBase ObtainCode
    (
        ValueCache<bool> getIsHollow,
        ValueCache<Size> getSize,
        ValueCache<TypeBase> getType,
        ValueCache<CodeBase> getCode
    )
    {
        if(getCode != null)
            return getCode.Value;
// ReSharper disable ExpressionIsAlwaysNull
        var isHollow = TryObtainIsHollow(getIsHollow, getSize, getType, getCode);
// ReSharper restore ExpressionIsAlwaysNull
        if(isHollow == true)
            return CodeBase.Void;
        Tracer.AssertionFailed("Code cannot be determined", ToString);
        return null;
    }

    TypeBase ObtainType
    (
        ValueCache<bool> getIsHollow,
        ValueCache<Size> getSize,
        ValueCache<TypeBase> getType,
        ValueCache<CodeBase> getCode,
        Root rootContext
    )
    {
        if(getType != null)
            return getType.Value;
// ReSharper disable ExpressionIsAlwaysNull
        var isHollow = TryObtainIsHollow(getIsHollow, getSize, getType, getCode);
// ReSharper restore ExpressionIsAlwaysNull
        if(isHollow == true)
            return rootContext.VoidType;
        Tracer.AssertionFailed("Type cannot be determined", ToString);
        return null;
    }

    Size ObtainSize
    (
        ValueCache<bool> getIsHollow,
        ValueCache<Size> getSize,
        ValueCache<TypeBase> getType,
        ValueCache<CodeBase> getCode
    )
    {
        var result = TryObtainSize(getIsHollow, getSize, getType, getCode);
        (result != null).Assert(() => "Size cannot be determined " + ToString());
        return result;
    }

    static Size TryObtainSize
    (
        ValueCache<bool> getIsHollow,
        ValueCache<Size> getSize,
        ValueCache<TypeBase> getType,
        ValueCache<CodeBase> getCode
    )
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
        ValueCache<CodeBase> getCode
    )
    {
        var result = TryObtainIsHollow(getIsHollow, getSize, getType, getCode);
        if(result != null)
            return result.Value;
        Tracer.AssertionFailed("It cannot be obtained if it is hollow.", ToString);
        return false;
    }

    static bool? TryObtainIsHollow
    (
        ValueCache<bool> getIsHollow,
        ValueCache<Size> getSize,
        ValueCache<TypeBase> getType,
        ValueCache<CodeBase> getCode
    ) =>
        getIsHollow?.Value ?? getSize?.Value.IsZero ?? getType?.Value.IsHollow ?? getCode?.Value.IsEmpty;

    Closures ObtainClosures
    (
        ValueCache<bool> getIsHollow,
        ValueCache<Size> getSize,
        ValueCache<TypeBase> getType,
        ValueCache<CodeBase> getCode,
        ValueCache<Closures> getArgs
    )
    {
        if(getArgs != null)
            return getArgs.Value;
        if(getCode != null)
            return getCode.Value.Closures;
// ReSharper disable ExpressionIsAlwaysNull
        if(TryObtainIsHollow(getIsHollow, getSize, getType, getCode) == true)
// ReSharper restore ExpressionIsAlwaysNull
            return Closures.Void();

        Tracer.AssertionFailed("Closures cannot be determined", ToString);
        return null;
    }

    internal void Update(Result result)
    {
        Issues = Issues.Union(result.Issues).ToArray();

        if(HasIssue)
            Reset(Category.All - Category.Closures);
        else
        {
            if(result.HasIsHollow)
                Data.IsHollow = result.IsHollow;

            if(result.HasSize)
                Data.Size = result.Size;

            if(result.HasType)
                Data.Type = result.Type;

            if(result.HasCode)
                Data.Code = result.Code;
        }

        if(result.HasClosures)
            Data.Closure = result.Closures;

        Data.PendingCategory = Data.PendingCategory - result.CompleteCategory;

        AssertValid();
    }

    Result Filter(Category category)
    {
        var result = new Result
        (
            CompleteCategory & category,
            Issues,
            () => IsHollow.AssertValue(),
            () => Size,
            () => Type,
            () => Code,
            () => Closures);
        result.Data.PendingCategory = result.Data.PendingCategory & category;
        return result;
    }

    void AssertValid()
    {
        if(IsDirty)
            return;

        ((CompleteCategory & PendingCategory) == Category.None).Assert();

        if(HasIssue)
        {
            (!HasIsHollow).Assert();
            (!HasSize).Assert();
            (!HasType).Assert();
            (!HasCode).Assert();
            return;
        }

        if(HasIsHollow && HasSize)
            (Size.IsZero == IsHollow).Assert
                (() => "Size and IsHollow differ: " + Dump());
        if(HasIsHollow && HasType && Type.HasQuickSize)
            (Type.IsHollow == IsHollow).Assert
                (() => "Type and IsHollow differ: " + Dump());
        if(HasIsHollow && HasCode)
            (Code.IsHollow == IsHollow).Assert
                (() => "Code and IsHollow differ: " + Dump());
        if(HasSize && HasType && Type.HasQuickSize)
            (Type.Size == Size).Assert(() => "Type and Size differ: " + Dump());
        if(HasSize && HasCode)
            (Code.Size == Size).Assert(() => "Code and Size differ: " + Dump());
        if(HasType && HasCode && Type.HasQuickSize)
            (Code.Size == Type.Size).Assert(() => "Code and Type differ: " + Dump());
        if(HasClosures && HasCode)
            Code.Closures.IsEqual(Closures).Assert
                (() => "Code and Closures differ: " + Dump());
    }

    void Add(Result other) => Add(other, CompleteCategory);

    void Add(Result other, Category category)
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
            (category <= other.CompleteCategory).Assert
            (() => nameof(other).DumpValue(other) + ", " + nameof(category).DumpValue(category)
            );

            (category <= CompleteCategory).Assert
            (() => "this".DumpValue(this) + ", " + nameof(category).DumpValue(category)
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
                Type = Type.Pair(other.Type);
            else if(HasType)
                Type = null;

            if(category.HasCode)
                Code = Code + other.Code;
            else if(HasCode)
                Code = null;
        }

        if(category.HasClosures)
            Closures = Closures.Sequence(other.Closures);
        else if(HasClosures)
            Closures = null;

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
        if(category.HasClosures)
            Closures = null;
        IsDirty = false;
    }

    internal Result Sequence(Result second)
    {
        var result = Clone;
        result.Add(second);
        return result;
    }

    internal Result ReplaceArg(ResultCache resultCache)
        => HasArg? InternalReplaceArg(resultCache) : this;

    internal Result ReplaceArg(ResultCache.IResultProvider provider)
        => HasArg? InternalReplaceArg(new(provider)) : this;

    internal Result ReplaceArg(Func<Category, Result> getArgs)
        => HasArg? InternalReplaceArg(new(getArgs)) : this;

    Result InternalReplaceArg(ResultCache getResultForArg)
    {
        var result = new Result
        {
            Issues = Issues, IsHollow = IsHollow, Size = Size, Type = Type, Code = Code, Closures = Closures
            , IsDirty = true
        };

        var categoryForArg = CompleteCategory & Category.Code.WithClosures;
        if(HasCode)
            categoryForArg |= Category.Type;

        var resultForArg = getResultForArg & categoryForArg;
        if(resultForArg != null)
        {
            result.Issues = result.Issues.Union(resultForArg.Issues).ToArray();

            if(HasCode)
                result.Code = Code.ReplaceArg(resultForArg);
            if(HasClosures)
                result.Closures = Closures.WithoutArg() + resultForArg.Closures;
        }

        result.IsDirty = false;
        return result;
    }

    internal Result ReplaceAbsolute<TRefInCode>
        (TRefInCode refInCode, Func<CodeBase> replacementCode, Func<Closures> replacementRefs)
        where TRefInCode : IContextReference
    {
        if(HasClosures && !Closures.Contains(refInCode))
            return this;

        if(!HasCode && !HasClosures)
            return this;

        var result = new Result
        {
            Issues = Issues, IsHollow = IsHollow, Size = Size, Type = Type, IsDirty = true
        };
        if(HasCode)
            result.Code = Code.ReplaceAbsolute(refInCode, replacementCode);
        if(HasClosures)
            result.Closures = Closures.Without(refInCode).Sequence(replacementRefs());
        result.IsDirty = false;
        return result;
    }

    internal Result ReplaceAbsolute<TRefInCode>(TRefInCode refInCode, Func<Category, Result> getReplacement)
        where TRefInCode : IContextReference
    {
        if(HasClosures && !Closures.Contains(refInCode))
            return this;

        if(!HasCode && !HasClosures)
            return this;

        var replacement = getReplacement(CompleteCategory - Category.Size - Category.Type);
        var result = new Result
        {
            Issues = Issues, IsHollow = IsHollow, Size = Size, Type = Type, IsDirty = true
        };
        if(HasCode)
            result.Code = Code.ReplaceAbsolute(refInCode, () => replacement.Code);
        if(HasClosures)
            result.Closures = Closures.Without(refInCode).Sequence(replacement.Closures);
        result.IsDirty = false;
        return result;
    }

    internal Result ReplaceRelative<TRefInCode>
        (TRefInCode refInCode, Func<CodeBase> replacementCode, Func<Closures> replacementRefs)
        where TRefInCode : IContextReference
    {
        if(HasClosures && !Closures.Contains(refInCode))
            return this;

        if(!HasCode && !HasClosures)
            return this;

        var result = new Result
        {
            Issues = Issues, IsHollow = IsHollow, Size = Size, Type = Type
        };
        if(HasCode)
            result.Code = Code.ReplaceRelative(refInCode, replacementCode);
        if(HasClosures)
            result.Closures = Closures.Without(refInCode).Sequence(replacementRefs());
        return result;
    }

    internal Result ReplaceRefsForFunctionBody(CodeBase replacement)
    {
        if(!HasCode)
            return this;
        if(SmartClosures.Count == 0)
            return this;
        var result = Clone;
        result.IsDirty = true;
        result.Code = SmartClosures.ReplaceRefsForFunctionBody(Code, replacement);
        result.Closures = replacement.Closures;
        result.IsDirty = false;
        return result;
    }

    internal Result LocalBlock(Category category)
        => AutomaticDereferenceResult.InternalLocalBlock(category.WithType);

    Result InternalLocalBlock(Category category)
    {
        if(HasIssue)
            return this;

        if(!category.HasCode && !category.HasClosures)
            return this;

        var result = this & category;
        var copier = Type.Copier(category);
        if(category.HasCode)
            result.Code = Code.LocalBlock(copier.Code);
        if(category.HasClosures)
            result.Closures = Closures.Sequence(copier.Closures);
        return result;
    }

    internal Result Conversion(TypeBase target)
    {
        var conversion = Type.Conversion(CompleteCategory, target);
        return conversion.ReplaceArg(this);
    }

    internal BitsConst Evaluate(IExecutionContext context)
    {
        Closures.IsNone.Assert(Dump);
        var result = Align.LocalBlock(CompleteCategory);
        return result.Code.Evaluate(context);
    }

    [DebuggerHidden]
    public static Result operator &(Result result, Category category) => result.Filter(category);

    [DebuggerHidden]
    public static Result operator |(Result aResult, Result bResult)
    {
        (aResult.CompleteCategory & bResult.CompleteCategory).IsNone.Assert();
        var result = aResult.Clone;
        result.Update(bResult);
        return result;
    }

    public static Result operator +(Result aResult, Result bResult) => aResult.Sequence(bResult);

    [DebuggerHidden]
    internal void AssertVoidOrValidReference()
    {
        var size = FindSize;
        if(size != null)
            (size.IsZero || size == Root.DefaultRefAlignParam.RefSize).Assert
                (() => "Expected size: 0 or RefSize\n" + Dump());

        if(HasType)
            (Type is VoidType || Type is PointerType).Assert
                (() => "Expected type: Void or PointerType\n" + Dump());
    }

    [DebuggerHidden]
    internal void AssertValidReference()
    {
        var size = FindSize;
        if(size != null)
            (size == Root.DefaultRefAlignParam.RefSize).Assert
                (() => "Expected size: RefSize\n" + Dump());

        if(HasType)
            (Type is PointerType).Assert(() => "Expected type: PointerType\n" + Dump());
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
            (size == Root.DefaultRefAlignParam.RefSize).Assert
                (() => "Expected size: 0 or RefSize\n" + Dump());
        }

        if(HasType)
            (Type is PointerType).Assert(() => "Expected type: PointerType\n" + Dump());
    }

    internal Result AddToReference(Func<Size> func) => Change(code => code.ReferencePlus(func()));

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
        var result = ((IConversion)Type).Result(CompleteCategory);
        return result.ReplaceArg(this);
    }

    internal Result SmartUn<T>()
        where T : IConversion => Type is T? Un() : this;

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
                ? new(CompleteCategory - Category.Type, getCode: () => Code.DePointer(size))
                : this;

    internal Result ConvertToConverter(TypeBase source)
        => source.IsHollow || (!HasClosures && !HasCode)
            ? this
            : ReplaceAbsolute(source.CheckedReference, source.ArgResult);

    internal Result AddCleanup(Result cleanup)
    {
        if(!HasCode || !HasClosures || cleanup.IsEmpty)
            return this;

        if(HasClosures && !cleanup.Closures.IsNone)
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
                (CompleteCategory, () => Code.InvalidConversion(destination.Size), () => Closures);

    internal bool IsValidOrIssue(Category category)
        => HasIssue || category <= CompleteCategory;
}