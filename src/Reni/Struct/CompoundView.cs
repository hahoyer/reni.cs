using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.SyntaxTree;
using Reni.TokenClasses;
using Reni.Type;
using Reni.Validation;

namespace Reni.Struct;

sealed class CompoundView : DumpableObject, ValueCache.IContainer
{
    sealed class RecursionWhileObtainingCompoundSizeException : Exception
    {
        [EnableDump]
        readonly CompoundView CompoundView;

        public RecursionWhileObtainingCompoundSizeException(CompoundView compoundView)
            => CompoundView = compoundView;
    }

    sealed class ConverterAccess : DumpableObject, IConversion
    {
        [EnableDump]
        readonly FunctionType Parent;

        [EnableDump]
        readonly CompoundType Type;

        public ConverterAccess(FunctionType parent, CompoundType type)
        {
            Parent = parent;
            Type = type;
        }

        Result IConversion.Execute(Category category)
        {
            var innerResult = ((IConversion)Parent).Execute(category);
            var conversion = Type.Make.Pointer.GetMutation(Parent);
            var result = innerResult.ReplaceArguments(conversion);
            return result;
        }

        TypeBase IConversion.Source => Type.Make.Pointer;
    }

    static int NextObjectId;

    [EnableDump]
    [Node]
    internal readonly Compound Compound;

    [EnableDump]
    [Node]
    internal readonly int ViewPosition;

    [Node]
    readonly FunctionCache<int, AccessFeature> AccessFeaturesCache;

    [Node]
    readonly FunctionCache<int, FieldAccessType> FieldAccessTypeCache;

    [Node]
    readonly FunctionCache<FunctionSyntax, FunctionBodyType> FunctionBodyTypeCache;

    [Node]
    readonly ValueCache<CompoundType> TypeCache;

    [Node]
    readonly ValueCache<CompoundContext> CompoundContextCache;

    bool IsObtainCompoundSizeActive;

    bool IsDumpPrintResultViaObjectActive;

    bool IsEndPosition => Compound.Syntax.GetIsEndPosition(ViewPosition);

    [DisableDump]
    internal ContextBase Context
        => Compound.Parent.GetCompoundPositionContext(Compound.Syntax, ViewPosition);

    [DisableDump]
    internal CompoundType Type => TypeCache.Value;

    [DisableDump]
    TypeBase IndexType => Compound.IndexType;

    [DisableDump]
    internal CompoundContext CompoundContext => CompoundContextCache.Value;

    [DisableDump]
    internal Size? CompoundViewSize
    {
        get
        {
            if(IsObtainCompoundSizeActive)
                throw new RecursionWhileObtainingCompoundSizeException(this);

            try
            {
                IsObtainCompoundSizeActive = true;
                var result = Compound.Size(ViewPosition);
                IsObtainCompoundSizeActive = false;
                return result;
            }
            catch(RecursionWhileObtainingCompoundSizeException)
            {
                IsObtainCompoundSizeActive = false;
                return null;
            }
        }
    }

    [DisableDump]
    internal bool IsHollow => Compound.IsHollow(ViewPosition);

    [DisableDump]
    internal Root Root => Compound.Root;

    [DisableDump]
    internal IEnumerable<IConversion> ConverterFeatures
        => Compound
            .Syntax
            .ConverterFunctions
            .Select(ConversionFunction);

    [DisableDump]
    internal IEnumerable<IConversion> KernelPartConversions
    {
        get
        {
            var meansKernelPart = Compound.BracketsSetup.MeansKernelPart;
            var statements = Compound.Syntax.Statements;
            var kernelPartPositions = Compound
                .Syntax
                .EndPosition
                .Select()
                .Where(position => IsValidKernelPart(statements[position]) ?? meansKernelPart)
                .ToArray();
            if(kernelPartPositions.Any())
            {
                var conversion = new Conversion(category => GetPartConverter(category, kernelPartPositions)
                    , Type.Make.Pointer);
                return [conversion];
            }

            return [];
        }
    }

    [DisableDump]
    internal string DumpPrintTextOfType
        => Compound
            .Syntax
            .EndPosition
            .Select(position => Compound.AccessType(ViewPosition, position).OverView.DumpPrintText)
            .Stringify(", ")
            .Surround("(", ")");

    [DisableDump]
    internal IEnumerable<IConversion> MixinConversions
        => Compound
            .Syntax
            .MixInDeclarations
            .Select(AccessFeature);

    [DisableDump]
    ContextReferenceType ContextReferenceType
        => this.CachedValue(() => new ContextReferenceType(this));

    [DisableDump]
    internal IEnumerable<string> DeclarationOptions
    {
        get
        {
            yield return DumpPrintToken.TokenId;
            yield return AtToken.TokenId;

            foreach(var name in Compound.Syntax.AllNames)
                yield return name;
        }
    }

    internal Issue[] Issues => Compound.GetIssues(ViewPosition);

    [UsedImplicitly]
    internal bool HasIssues => Issues.Any();

    [DisableDump]
    [UsedImplicitly]
    internal int Count => Compound.Syntax.Statements.Length;

    internal CompoundView(Compound compound, int viewPosition)
        : base(NextObjectId++)
    {
        Compound = compound;
        ViewPosition = viewPosition;
        CompoundContextCache = new(() => new(this));
        TypeCache = new(() => new(this));
        AccessFeaturesCache = new(position => new(this, position));
        FieldAccessTypeCache = new(position => new(this, position));
        FunctionBodyTypeCache = new(syntax => new(this, syntax));
        StopByObjectIds();
    }

    ValueCache ValueCache.IContainer.Cache { get; } = new();

    protected override string GetNodeDump()
    {
        var result = base.GetNodeDump();
        result += $"({Context.GetContextIdentificationDump()})";
        if(HasIssues)
            result += $".AllIssues[{Issues.Length}]";
        return result;
    }

    Result GetPartConverter(Category category, int[] kernelPartPositions)
        => Root
            .ConcatResult
            (
                category
                , kernelPartPositions.Length
                , (category, position)
                    => AccessValueViaObject(category, kernelPartPositions[position])
            );

    bool? IsValidKernelPart(IStatementSyntax statement)
    {
        if(statement is not DeclarationSyntax declarationSyntax)
            return null;
        var declarer = declarationSyntax.Declarer;
        if(declarer.IsConverterSyntax)

            NotImplementedMethod(statement);
        return default;
    }

    internal string GetContextDump() => Compound.Syntax.GetPositionDump(ViewPosition);

    internal string GetCompoundIdentificationDump() => Context.GetContextIdentificationDump();

    internal TypeBase FunctionalType(FunctionSyntax syntax) => FunctionBodyTypeCache[syntax];

    [PublicAPI]
    internal AccessFeature AccessFeature(int position) => AccessFeaturesCache[position];

    [PublicAPI]
    internal TypeBase AccessType(int position)
    {
        var result = Compound.AccessType(ViewPosition, position);
        if(result.OverView.IsHollow)
            return result;

        return FieldAccessTypeCache[position];
    }

    internal Result AccessViaPositionExpression(Category category, Result rightResult)
    {
        var position = rightResult
            .GetConversion(IndexType)
            .GetSmartUn<PointerType>()
            .GetValue(Compound.Root.ExecutionContext)
            .ToInt32();
        return AccessViaObjectPointer(category, position);
    }

    internal Size FieldOffset(int position)
        => Compound.FieldOffsetFromAccessPoint(ViewPosition, position);

    internal Result GetDumpPrintResultViaObject(Category category)
    {
        if(IsDumpPrintResultViaObjectActive)
            return Root.VoidType.GetResult(category, () => "?".GetDumpPrintTextCode());

        IsDumpPrintResultViaObjectActive = true;
        var result = Root.ConcatPrintResult
        (
            category,
            ViewPosition,
            GetDumpPrintResultViaObject
        );
        IsDumpPrintResultViaObjectActive = false;
        return result;
    }

    Result AccessViaObjectPointer(Category category, int position)
    {
        var resultType = AccessType(position);
        if(resultType.OverView.IsHollow)
            return resultType.GetResult(category);

        return (Type.OverView.IsHollow? Type : Type.Make.Pointer).GetMutation(resultType) & category;
    }

    internal Result AccessViaObject(Category category, int position)
    {
        var resultType = AccessType(position);
        if(resultType.OverView.IsHollow)
            return resultType.GetResult(category);

        return resultType.GetResult(category, Type.GetObjectResult);
    }

    internal Result AccessValueViaObject(Category category, int position)
    {
        var resultType = ValueType(position);
        if(resultType.OverView.IsHollow)
            return resultType.GetResult(category);

        return resultType.GetResult(category, getCodeAndRefs);

        Result getCodeAndRefs(Category c)
            => Type
                .GetObjectResult(c)
                .AddToReference(() => FieldOffset(position))
                .GetDereferencedAlignedResult(resultType.OverView.Size);
    }


    internal Result ReplaceObjectPointerByContext(Result target)
    {
        var reference = (Type.OverView.IsHollow? Type : Type.Make.Pointer).Make.CheckedReference!;
        return target.ReplaceAbsolute(reference, ObjectPointerViaContext);
    }

    IConversion ConversionFunction(FunctionSyntax body)
    {
        IConversion result = new ConverterAccess(Function(body, Root.VoidType), Type);
        var source = result.Source;
        (source == Type.Make.Pointer).Assert(source.Dump);
        (source == result.GetResult(Category.Code).Code.ArgumentType).Assert();
        return result;
    }

    internal FunctionType Function(FunctionSyntax body, TypeBase argsType)
        => Compound
            .Root
            .GetFunctionInstance(this, body, argsType.AssertNotNull());

    internal IEnumerable<FunctionType> Functions(FunctionSyntax body)
        => Compound
            .Root
            .GetFunctionInstances(this, body);

    internal Result ObjectPointerViaContext(Category category)
    {
        if(IsHollow)
            return Type.GetResult(category);

        return (Type.OverView.IsHollow? Type : Type.Make.Pointer)
            .GetResult
            (
                category,
                ObjectPointerViaContext,
                () => Closures.Create(Compound)
            );
    }

    Result GetDumpPrintResultViaObject(Category category, int position)
    {
        var trace = ObjectId == -5 && position == 1;
        StartMethodDump(trace, category, position);
        try
        {
            BreakExecution();
            var tempQualifier = ValueType(position);
            var accessType = tempQualifier.OverView.IsHollow? tempQualifier : tempQualifier.Make.Pointer;
            var genericDumpPrintResult = accessType.GetGenericDumpPrintResult(category);
            Dump("genericDumpPrintResult", genericDumpPrintResult);
            BreakExecution();
            return ReturnMethodDump
            (
                genericDumpPrintResult.ReplaceAbsolute
                    (accessType.Make.CheckedReference!, c => AccessValueViaObject(c, position)));
        }
        finally
        {
            EndMethodDump();
        }
    }

    CodeBase ObjectPointerViaContext()
        => Compound.GetCode().GetReferenceWithOffset(CompoundViewSize! * -1);

    internal TypeBase ValueType(int position)
        => Compound
            .AccessType(ViewPosition, position)
            .AssertNotNull().Make.TypeForStructureElement;

    internal IImplementation? Find(Definable definable, bool publicOnly)
    {
        var position = Compound.Syntax.Find(definable.Id, publicOnly);
        return position == null? null : AccessFeature(position.Value);
    }

    internal Result AtTokenResult(Category category, Result rightResult)
        => AccessViaPositionExpression(category, rightResult)
            .ReplaceArguments(ObjectPointerViaContext);

    internal Result ContextOperatorResult(Category category)
        => ContextReferenceType.GetResult(category, ContextOperatorCode);

    CodeBase ContextOperatorCode()
        => IsHollow
            ? CodeBase.Void
            : Type.Make.ForcedReference
                .GetCode()
                .GetReferenceWithOffset(CompoundViewSize!);

    Result CreateElement(Category category, int index, TypeBase elementType)
        => AccessViaObjectPointer(category, index).GetConversion(elementType);
}
