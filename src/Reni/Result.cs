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
    static int NextObjectId = 1;

    /// <summary>
    ///     Regular result
    /// </summary>
    readonly ResultData Data;

    /// <summary>
    ///     Result in case of Issues
    /// </summary>
    readonly IssueData IssueData;

    Category PendingCategoryValue = Category.None;
    bool IsDirtyValue;

    bool HasSize => Size != null;
    bool HasType => Type != null;
    bool HasCode => Code != null;
    bool HasClosures => Closures != null;
    bool HasIsHollow => IsHollow != null;

    [Node]
    [EnableDumpWithExceptionPredicate]
    public Category CompleteCategory
    {
        get
        {
            var result = Category.None;

            if(HasCode)
                result |= Category.Code;
            if(HasClosures)
                result |= Category.Closures;
            if(HasIsHollow)
                result |= Category.IsHollow;
            if(HasType)
                result |= Category.Type;
            if(HasSize)
                result |= Category.Size;
            return result;
        }
    }

    /// <summary>
    ///     Is this an hollow object? With no data?
    /// </summary>
    [Node]
    [DebuggerHidden]
    public bool? IsHollow
    {
        get => IssueData.IsHollow ?? Data.IsHollow;
        set
        {
            Set(Category.IsHollow, value);
            AssertValid();
        }
    }

    [Node]
    [DebuggerHidden]
    public Size Size
    {
        get => IssueData.Size ?? Data.Size;
        set
        {
            Set(Category.Size, value);
            AssertValid();
        }
    }

    [Node]
    [DebuggerHidden]
    internal TypeBase Type
    {
        get => IssueData.Type ?? Data.Type;
        set
        {
            Set(Category.Type, value);
            AssertValid();
        }
    }

    [Node]
    [DebuggerHidden]
    internal CodeBase Code
    {
        get => IssueData.Code ?? Data.Code;
        set
        {
            Set(Category.Code, value);
            AssertValid();
        }
    }

    [Node]
    [DebuggerHidden]
    internal Closures Closures
    {
        get => IssueData.Closure ?? Data.Closures;
        set
        {
            Set(Category.Closures, value);
            AssertValid();
        }
    }

    internal bool? FindIsHollow => HasIsHollow? Data.IsHollow : FindSize?.IsZero;

    [PublicAPI]
    bool? QuickFindIsHollow => HasIsHollow? Data.IsHollow : QuickFindSize?.IsZero;

    bool SmartIsHollow
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
        => HasSize? Size :
            HasCode? Code.Size :
            HasType? Type.Size : null;

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

    [PublicAPI]
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
        => HasClosures? Closures :
            HasCode? Code.Closures : null;

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
        get => IsDirtyValue;
        set
        {
            IsDirtyValue = value;
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

    bool HasArg
        => HasClosures? Closures.HasArg : HasCode && Code.HasArg;

    public Category PendingCategory
    {
        get => PendingCategoryValue;
        set
        {
            PendingCategoryValue = value;
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
                CompleteCategory, () => Closures
                , () => Code.BitCast(alignedSize)
                , () => Type.Align
                , () => alignedSize
                , () => IsHollow.AssertValue());
            return result;
        }
    }

    public bool HasIssue => IssueData.HasIssue;

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
    Result Clone => Filter(CompleteCategory);

    [DisableDump]
    internal Result AutomaticDereferenceResult
    {
        get
        {
            if(HasIssue)
                return this;

            HasType.Assert(() => "Dereference requires type category:\n " + Dump());

            var result = this;
            while(result.HasIssue != true && result.Type.IsWeakReference)
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
    [PublicAPI]
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

    public Issue[] Issues
    {
        get => IssueData.Issues;
        private set => IssueData.Issues = value;
    }

    internal Result(Category category, Issue[] issues)
        : this
        (
            category,
            issues,
            null,
            null,
            null,
            null,
            null) { }

    internal Result(Category category, Issue recentIssue)
        : this(category, new[] { recentIssue }) { }

    internal Result
    (
        Category category,
        Func<Closures> getClosures = null,
        Func<CodeBase> getCode = null,
        Func<TypeBase> getType = null,
        Func<Size> getSize = null,
        Func<bool> getIsHollow = null
    )
        : this
        (
            category,
            null,
            getIsHollow,
            getSize,
            getType,
            getCode,
            getClosures) { }

    Result
    (
        Category category,
        Issue[] issues,
        Func<bool> getIsHollow,
        Func<Size> getSize,
        Func<TypeBase> getType,
        Func<CodeBase> getCode,
        Func<Closures> getClosures
    )
        : base(NextObjectId++)
    {
        IssueData = new() { Category = category, Issues = issues };
        Data = new
        (
            issues?.Any() == true? Category.None : category
            , getType
            , getCode
            , getClosures
            , getSize
            , getIsHollow, ToString
        );

        AssertValid();
        StopByObjectIds();
    }

    Result IAggregateable<Result>.Aggregate(Result other) => this + other;

    public override string DumpData()
    {
        var result = "";
        if(PendingCategory != Category.None)
            result += "\nPendingCategory=" + PendingCategory.Dump();
        if(CompleteCategory != Category.None)
            result += "\nCompleteCategory=" + CompleteCategory.Dump();
        if(HasIssue)
            result += "\nIssues=" + Tracer.Dump(Issues);
        if(HasIsHollow)
            result += "\nIsHollow=" + Tracer.Dump(IsHollow);
        if(HasSize)
            result += "\nSize=" + Tracer.Dump(Size);
        if(HasType)
            result += "\nType=" + Tracer.Dump(Type);
        if(HasClosures)
            result += "\nClosures=" + Tracer.Dump(Closures);
        if(HasCode)
            result += "\nCode=" + Tracer.Dump(Code);
        if(result == "")
            return "";
        return result.Substring(1);
    }

    void Set(Category category, object value)
    {
        var isDirty = IsDirty;
        IsDirty = true;
        Data.Set(category, value);
        IssueData.Set(category, !Equals(value, default));
        if(HasIssue)
            Data.Reset(~category);
        IsDirty = isDirty;
    }

    internal void Update(Result result)
    {
        Issues = T(Issues, result.Issues).ConcatMany().ToArray();

        if(HasIssue)
        {
            IssueData.Set(result.CompleteCategory);
            Data.Reset(Category.All);
        }
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

            if(result.HasClosures)
                Data.Closures = result.Closures;
        }

        PendingCategory = PendingCategory.Without(result.CompleteCategory);

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
        result.PendingCategory &= category;
        return result;
    }

    void AssertValid()
    {
        if(IsDirty)
            return;

        ((CompleteCategory & PendingCategory) == Category.None).Assert();

        if(HasIssue)
        {
            if(!IssueData.Category.HasIsHollow())
                Data.IsHollow.AssertIsNull();
            if(!IssueData.Category.HasSize())
                Data.Size.AssertIsNull();
            if(!IssueData.Category.HasType())
                Data.Type.AssertIsNull();
            if(!IssueData.Category.HasCode())
                Data.Code.AssertIsNull();
            if(!IssueData.Category.HasClosures())
                Data.Closures.AssertIsNull();
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
            Code.Closures.IsEqual(Closures)
                .Assert(() => "Code and Closures differ: " + Dump());
    }

    void Add(Result other) => Add(other, CompleteCategory);

    void Add(Result other, Category category)
    {
        var isDirty = IsDirty;
        IsDirty = true;

        Issues = T(Issues, other.Issues).ConcatMany().ToArray();

        var hasIssue = HasIssue;

        if(!hasIssue)
        {
            other.CompleteCategory.Contains(category).Assert
            (() => nameof(other).DumpValue(other) + ", " + nameof(category).DumpValue(category)
            );

            CompleteCategory.Contains(category).Assert
            (() => "this".DumpValue(this) + ", " + nameof(category).DumpValue(category)
            );

            if(category.HasIsHollow())
                IsHollow = SmartIsHollow && other.SmartIsHollow;
            else if(HasIsHollow)
                IsHollow = null;

            if(category.HasSize())
                Size += other.Size;
            else if(HasSize)
                Size = null;

            if(category.HasType())
                Type = Type.Pair(other.Type);
            else if(HasType)
                Type = null;

            if(category.HasCode())
                Code = Code + other.Code;
            else if(HasCode)
                Code = null;
        }

        if(category.HasClosures())
            Closures = Closures.AssertNotNull().Sequence(other.Closures);
        else if(HasClosures)
            Closures = null;

        IsDirty = isDirty;
    }

    void Reset(Category category)
    {
        var isDirty = IsDirty;
        IsDirty = true;
        Data.Reset(category);
        IssueData.Set(category, false);
        IsDirty = isDirty;
    }

    internal Result Sequence(Result second)
    {
        var result = Clone;
        result.Add(second);
        return result;
    }

    internal Result ReplaceArg(ResultCache resultCache)
        => HasArg? InternalReplaceArg(resultCache) : this;

    [PublicAPI]
    internal Result ReplaceArg(ResultCache.IResultProvider provider)
        => HasArg? InternalReplaceArg(new(provider)) : this;

    internal Result ReplaceArg(Func<Category, Result> getArgs)
        => HasArg? InternalReplaceArg(new(getArgs)) : this;

    Result InternalReplaceArg(ResultCache getResultForArg)
    {
        var categoryForArg = CompleteCategory & (Category.Code | Category.Closures);
        if(HasCode)
            categoryForArg |= Category.Type;

        var resultForArg = getResultForArg & categoryForArg;
        if(resultForArg == null)
            return this;

        var result = Clone;
        result.IsDirty = true;

        result.Issues = T(result.Issues, resultForArg.Issues).ConcatMany().ToArray();

        if(HasCode)
            result.Code = Code.ReplaceArg(resultForArg);
        if(HasClosures)
            result.Closures = Closures.WithoutArg() + resultForArg.Closures;
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

        var result = Clone;
        result.IsDirty = true;

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

        var replacement = getReplacement(CompleteCategory.Without(Category.Size | Category.Type));
        var result = Clone;
        result.IsDirty = true;

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

        var result = Clone;
        result.IsDirty = true;
        if(HasCode)
            result.Code = Code.ReplaceRelative(refInCode, replacementCode);
        if(HasClosures)
            result.Closures = Closures.Without(refInCode).Sequence(replacementRefs());
        result.IsDirty = false;
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
        => AutomaticDereferenceResult.InternalLocalBlock(category | Category.Type);

    Result InternalLocalBlock(Category category)
    {
        if(HasIssue)
            return this;

        if(!category.HasCode() && !category.HasClosures())
            return this;

        var result = this & category;
        var copier = Type.Copier(category);
        if(category.HasCode())
            result.Code = Code.LocalBlock(copier.Code);
        if(category.HasClosures())
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
        ((aResult.CompleteCategory & bResult.CompleteCategory) == Category.None).Assert();
        var result = aResult.Clone;
        result.Update(bResult);
        return result;
    }

    public static Result operator +(Result aResult, Result bResult) => aResult.Sequence(bResult);

    [DebuggerHidden]
    [PublicAPI]
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
    [PublicAPI]
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
    [PublicAPI]
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

    [PublicAPI]
    internal Result DereferencedAlignedResult(Size size)
        => HasIssue? this :
            HasCode? new(CompleteCategory.Without(Category.Type), getCode: () => Code.DePointer(size)) :
            this;

    internal Result ConvertToConverter(TypeBase source)
        => source.IsHollow || (!HasClosures && !HasCode)
            ? this
            : ReplaceAbsolute(source.CheckedReference, source.ArgResult);

    [PublicAPI]
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
        => HasIssue || CompleteCategory.Contains(category);
}