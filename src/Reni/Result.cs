using System.Diagnostics;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Helper;
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
    ///     GetResult in case of Issues
    /// </summary>
    readonly IssueData IssueData;

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
    ///     Is this a hollow object? With no data?
    /// </summary>
    [Node]
    [DebuggerHidden]
    public bool? IsHollow
    {
        get => IssueData.IsHollow ?? Data.IsHollow;
        set
        {
            if(IsHollow == value)
                return;

            Set(Category.IsHollow, value);

            if(value == true)
                Size = Size.Zero;

            AssertValid();
        }
    }

    [Node]
    [DebuggerHidden]
    public Size? Size
    {
        get => IssueData.Size ?? Data.Size;
        set
        {
            if(Size == value)
                return;

            Set(Category.Size, value);
            IsHollow = value == Size.Zero;

            AssertValid();
        }
    }

    [Node]
    [DebuggerHidden]
    internal TypeBase? Type
    {
        get => IssueData.Type ?? Data.Type;
        set
        {
            if(Type == value)
                return;
            (value != null).Assert();

            Set(Category.Type, value);
            Size = Type!.Size;

            AssertValid();
        }
    }

    [Node]
    [DebuggerHidden]
    internal CodeBase? Code
    {
        get => IssueData.Code ?? Data.Code;
        set
        {
            if(Code == value)
                return;
            (value != null).Assert();
            Set(Category.Code, value);
            AssertValid();
        }
    }

    [Node]
    [DebuggerHidden]
    internal Closures? Closures
    {
        get => IssueData.Closure ?? Data.Closures;
        set
        {
            if(Closures == value)
                return;
            (value != null).Assert();
            Set(Category.Closures, value);
            AssertValid();
        }
    }

    internal bool? FindIsHollow => HasIsHollow? Data.IsHollow : FindSize?.IsZero;

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

    internal Size? FindSize
        => HasSize? Size :
            HasCode? Code!.Size :
            HasType? Type!.Size : null;

    internal Closures? FindClosures
        => HasClosures? Closures :
            HasCode? Code!.Closures : null;

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

            return result!;
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
            if(HasSize && !Size!.IsZero)
                return false;
            if(HasType && !(Type! is VoidType))
                return false;
            if(HasCode && !Code!.IsEmpty)
                return false;
            if(HasClosures && !Closures!.IsNone)
                return false;
            return true;
        }
    }

    bool HasArguments
        => HasClosures? Closures!.HasArguments : HasCode && Code!.HasArguments;

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
            var alignedSize = size.GetAlign(alignBits);
            if(alignedSize == size)
                return this;

            var result = new Result
            (
                CompleteCategory, () => Closures
                , () => Code?.GetBitCast(alignedSize)
                , () => Type?.Align
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
                : (Type!.GetMutation(weakType) & CompleteCategory).ReplaceArguments(this);
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

            HasType.Expect(() => (null, "Dereference requires type category:\n " + Dump()));

            var result = this;
            while(result.HasIssue != true && result.Type!.IsWeakReference)
                result = result.DereferenceResult;
            return result;
        }
    }

    [DisableDump]
    internal Result DereferenceResult
        => Type
            .ExpectNotNull(() => (null, "Dereference requires type category:\n " + Dump()))
            .CheckedReference
            .ExpectNotNull(() => (null, $"Type {Type!.DumpPrintText} is not a reference type."))
            .Converter.GetResult(CompleteCategory)
            .ReplaceArguments(this);

    [DisableDump]
    [PublicAPI]
    internal Result UnalignedResult
        => ((AlignType)Type.ExpectNotNull(() => (null, "UnalignedResult requires type category:\n " + Dump())))
            .UnalignedResult(CompleteCategory)
            .ReplaceArguments(this);

    [DisableDump]
    internal Result LocalReferenceResult
    {
        get
        {
            if(HasIssue)
                return this;
            if(!HasCode && !HasType && !HasSize)
                return this;
            if(Type!.IsHollow)
                return this;
            if(Type is IReference)
                return this;
            return Type
                .GetLocalReferenceResult(CompleteCategory)
                .ReplaceArguments(this);
        }
    }

    public Issue[] Issues
    {
        get => IssueData.Issues;
        private set
        {
            IssueData.Issues = value
                .Distinct(Extension.Comparer<Issue>((x, y) => x == y))
                .ToArray();
            IssueData.AssertValid();
        }
    }

    [DisableDump]
    internal Result AutomaticDereferencedAlignedResult
    {
        get
        {
            var destinationType = Type
                .ExpectNotNull(() => (null, "AutomaticDereferencedAlignedResult requires type category:\n " + Dump()))
                .AutomaticDereferenceType.Align;

            if(destinationType == Type)
                return this;

            return Type!
                .GetConversion(CompleteCategory, destinationType)
                .ReplaceArguments(this);
        }
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
        : this(category, [recentIssue]) { }

    internal Result
    (
        Category category,
        Func<Closures?>? getClosures = null,
        Func<CodeBase?>? getCode = null,
        Func<TypeBase?>? getType = null,
        Func<Size?>? getSize = null,
        Func<bool?>? getIsHollow = null
    )
        : this
        (
            category,
            [],
            getIsHollow,
            getSize,
            getType,
            getCode,
            getClosures) { }

    Result
    (
        Category category,
        Issue[] issues,
        Func<bool?>? getIsHollow,
        Func<Size?>? getSize,
        Func<TypeBase?>? getType,
        Func<CodeBase?>? getCode,
        Func<Closures?>? getClosures
    )
        : base(NextObjectId++)
    {
        IssueData = new(category, issues);
        Data = ResultExtension.CreateInstance
        (
            issues.Any()? Category.None : category
            , getType
            , getCode
            , getClosures
            , getSize
            , getIsHollow, ToString
        );

        Replenish();
        AssertValid();
        StopByObjectIds();
    }

    Result IAggregateable<Result>.Aggregate(Result? other) => (this + other).ExpectNotNull();

    public override string DumpData()
    {
        var result = "";
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

    void Replenish()
    {
        AssertValid();

        if(Closures == null && Code != null)
            Closures = Code.Closures;

        if(Size == null && Type != null)
            Size = Type.SmartSize;
        if(Size == null && Code != null)
            Size = Code.Size;

        if(IsHollow == null && Size != null)
            IsHollow = Size.IsZero;
    }

    void Set(Category category, object? value)
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
            IssueData.Set(CompleteCategory | result.CompleteCategory);
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
            () => Closures
        );
        return result;
    }

    void AssertValid()
    {
        if(IsDirty)
            return;

        IssueData.AssertValid(IssueData.Issues);

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
            (Size!.IsZero == IsHollow).Assert
                (() => "Size and IsHollow differ: " + Dump());
        if(HasIsHollow && HasType && Type!.HasQuickSize)
            (Type.IsHollow == IsHollow).Assert
                (() => "Type and IsHollow differ: " + Dump());
        if(HasIsHollow && HasCode)
            (Code!.IsHollow == IsHollow).Assert
                (() => "Code and IsHollow differ: " + Dump());
        if(HasSize && HasType && Type!.HasQuickSize)
            (Type.Size == Size).Assert(() => "Type and Size differ: " + Dump());
        if(HasSize && HasCode)
            (Code!.Size == Size).Assert(() => "Code and Size differ: " + Dump());
        if(HasType && HasCode && Type!.HasQuickSize)
            (Code!.Size == Type.Size).Assert(() => "Code and Type differ: " + Dump());
        if(HasClosures && HasCode)
            Code!.Closures.IsEqual(Closures!)
                .Assert(() => "Code and Closures differ: " + Dump());
    }

    void Add(Result other) => Add(other, CompleteCategory & other.CompleteCategory);

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
            else if(HasIsHollow && IsHollow == true)
                IsHollow = null;

            if(category.HasSize())
                Size = Size! + other.Size!;
            else if(HasSize)
                Size = null;

            if(category.HasType())
                Type = Type!.GetPair(other.Type!);
            else if(HasType)
                Type = null;

            if(category.HasCode())
                Code = Code! + other.Code!;
            else if(HasCode)
                Code = null;
        }

        if(category.HasClosures())
            Closures = Closures.AssertNotNull().Sequence(other.Closures!);
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

    internal Result GetSequence(Result second)
    {
        var result = Clone;
        result.Add(second);
        return result;
    }

    internal Result ReplaceArguments(ResultCache resultCache)
        => HasArguments? InternalReplaceArguments(resultCache) : this;

    [PublicAPI]
    internal Result ReplaceArguments(ResultCache.IResultProvider provider)
        => HasArguments? InternalReplaceArguments(new(provider)) : this;

    internal Result ReplaceArguments(Func<Category, Result> getArguments)
        => HasArguments? InternalReplaceArguments(ResultCache.CreateInstance(getArguments)) : this;

    Result InternalReplaceArguments(ResultCache getResultForArguments)
    {
        var categoryForArgument = CompleteCategory & (Category.Code | Category.Closures);
        if(HasCode)
            categoryForArgument |= Category.Type;

        var resultForArgument = getResultForArguments & categoryForArgument;
        var result = Clone;
        result.IsDirty = true;

        result.Issues = T(result.Issues, resultForArgument.Issues).ConcatMany().ToArray();

        if(HasCode)
            result.Code = Code!.ReplaceArgument(resultForArgument);
        if(HasClosures)
            result.Closures = Closures!.WithoutArgument() + resultForArgument.Closures!;
        result.IsDirty = false;
        return result;
    }

    internal Result ReplaceAbsolute<TRefInCode>
        (TRefInCode refInCode, Func<CodeBase> replacementCode, Func<Closures> replacementRefs)
        where TRefInCode : IContextReference
    {
        if(HasClosures && !Closures!.Contains(refInCode))
            return this;

        if(!HasCode && !HasClosures)
            return this;

        var result = Clone;
        result.IsDirty = true;

        if(HasCode)
            result.Code = Code!.ReplaceAbsolute(refInCode, replacementCode);
        if(HasClosures)
            result.Closures = Closures!.Without(refInCode).Sequence(replacementRefs());
        result.IsDirty = false;
        return result;
    }

    internal Result ReplaceAbsolute<TRefInCode>(TRefInCode refInCode, Func<Category, Result> getReplacement)
        where TRefInCode : IContextReference
    {
        if(HasClosures && !Closures!.Contains(refInCode))
            return this;

        if(!HasCode && !HasClosures)
            return this;

        var replacement = getReplacement(CompleteCategory.Without(Category.Size | Category.Type));
        var result = Clone;
        result.IsDirty = true;

        if(HasCode)
            result.Code = Code!.ReplaceAbsolute(refInCode, () => replacement.Code!);
        if(HasClosures)
            result.Closures = Closures!.Without(refInCode).Sequence(replacement.Closures!);
        result.IsDirty = false;
        return result;
    }

    internal Result ReplaceRelative<TRefInCode>
        (TRefInCode refInCode, Func<CodeBase> replacementCode, Func<Closures> replacementRefs)
        where TRefInCode : IContextReference
    {
        if(HasClosures && !Closures!.Contains(refInCode))
            return this;

        if(!HasCode && !HasClosures)
            return this;

        var result = Clone;
        result.IsDirty = true;
        if(HasCode)
            result.Code = Code!.ReplaceRelative(refInCode, replacementCode);
        if(HasClosures)
            result.Closures = Closures!.Without(refInCode).Sequence(replacementRefs());
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
        result.Code = SmartClosures.ReplaceRefsForFunctionBody(Code!, replacement);
        result.Closures = replacement.Closures;
        result.IsDirty = false;
        return result;
    }

    internal Result GetLocalBlock(Category category)
        => AutomaticDereferenceResult.InternalLocalBlock(category);

    Result InternalLocalBlock(Category category)
    {
        if(HasIssue)
            return this;

        if(!category.HasCode() && !category.HasClosures())
            return this;

        category |= Category.Type;
        var result = (this & category).ExpectNotNull();
        var copier = Type!.GetCopier(category);
        if(category.HasCode())
            result.Code = Code!.GetLocalBlock(copier?.Code!);
        if(category.HasClosures())
            result.Closures = Closures!.Sequence(copier?.Closures!);
        return result;
    }

    internal Result GetConversion(TypeBase target)
        => Type
            .ExpectNotNull(() => (null, "GetConversion requires type category:\n " + Dump()))
            .GetConversion(CompleteCategory, target)
            .ReplaceArguments(this);

    internal BitsConst GetValue(IExecutionContext context)
    {
        (Closures?.IsNone != false).Assert(Dump);
        var result = Align.GetLocalBlock(CompleteCategory);
        return result
            .Code
            .ExpectNotNull(() => (null, "GetValue requires code category:\n " + Dump()))
            .GetValue(context);
    }

    [DebuggerHidden]
    public static Result operator &(Result result, Category category) => result.Filter(category);

    [DebuggerHidden]
    [Obsolete("", true)]
    public static Result operator |(Result aResult, Result bResult)
    {
        ((aResult.CompleteCategory & bResult.CompleteCategory) == Category.None).Assert();
        var result = aResult.Clone;
        result.Update(bResult);
        return result;
    }

    public static Result? operator +(Result? aResult, Result? bResult)
    {
        if(aResult == null)
            return bResult;
        if(bResult == null)
            return aResult;
        return aResult.GetSequence(bResult);
    }


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

    internal Result AddToReference(Func<Size> func) => Change(code => code.GetReferenceWithOffset(func()));

    Result Change(Func<CodeBase, CodeBase> func)
    {
        if(!HasCode)
            return this;
        var result = Clone;
        result.Code = func(Code!);
        return result;
    }

    Result Un()
    {
        var result = ((IConversion)Type.ExpectNotNull(() => (null, "Un requires type category:\n " + Dump())))
            .GetResult(CompleteCategory);
        return result.ReplaceArguments(this);
    }

    internal Result GetSmartUn<T>()
        where T : IConversion => Type is T? Un() : this;

    [PublicAPI]
    internal Result GetDereferencedAlignedResult(Size size)
        => HasIssue? this :
            HasCode? new(CompleteCategory.Without(Category.Type), getCode: () => Code!.GetDePointer(size)) :
            this;

    internal Result ConvertToConverter(TypeBase source)
        => source.IsHollow || (!HasClosures && !HasCode)
            ? this
            : ReplaceAbsolute(source.CheckedReference!, source.GetArgumentResult);

    [PublicAPI]
    internal Result GetWithCleanupAdded(Result cleanup)
    {
        if(!HasCode || !HasClosures || cleanup.IsEmpty)
            return this;

        if(HasClosures && !cleanup.Closures!.IsNone)
            NotImplementedMethod(cleanup);

        var result = Clone;
        if(HasCode)
            result.Code = result.Code!.GetWithCleanupAdded(cleanup.Code!);
        return result;
    }

    internal Result ArrangeCleanupCode()
    {
        if(!HasCode)
            return this;

        var newCode = Code?.ArrangeCleanupCode();
        if(newCode == null)
            return this;

        var result = Clone;
        result.Code = newCode;
        return result;
    }

    internal Result GetInvalidConversion(TypeBase destination)
        =>
            destination.GetResult
            (
                CompleteCategory
                , HasCode? () => Code!.GetInvalidConversion(destination.Size) : null
                , HasClosures? () => Closures : null
            );

    internal bool IsValidOrIssue(Category category)
        => HasIssue || CompleteCategory.Contains(category);
}