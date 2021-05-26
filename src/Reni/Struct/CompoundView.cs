using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.SyntaxTree;
using Reni.TokenClasses;
using Reni.Type;
using Reni.Validation;

namespace Reni.Struct
{
    sealed class CompoundView : DumpableObject, ValueCache.IContainer
    {
        sealed class RecursionWhileObtainingCompoundSizeException : Exception
        {
            [EnableDump]
            readonly CompoundView CompoundView;

            public RecursionWhileObtainingCompoundSizeException
                (CompoundView compoundView) => CompoundView = compoundView;
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
                var conversion = Type.Pointer.Mutation(Parent);
                var result = innerResult.ReplaceArg(conversion);
                return result;
            }

            TypeBase IConversion.Source => Type.Pointer;
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

        bool IsObtainCompoundSizeActive;

        [Node]
        readonly ValueCache<CompoundType> TypeCache;

        [Node]
        readonly ValueCache<CompoundContext> CompoundContextCache;

        bool IsDumpPrintResultViaObjectActive;

        internal CompoundView(Compound compound, int viewPosition)
            : base(NextObjectId++)
        {
            Compound = compound;
            ViewPosition = viewPosition;
            CompoundContextCache = new ValueCache<CompoundContext>
                (() => new CompoundContext(this));

            TypeCache = new ValueCache<CompoundType>(() => new CompoundType(this));

            AccessFeaturesCache
                = new FunctionCache<int, AccessFeature>
                    (position => new AccessFeature(this, position));

            FieldAccessTypeCache
                = new FunctionCache<int, FieldAccessType>
                    (position => new FieldAccessType(this, position));

            FunctionBodyTypeCache
                = new FunctionCache<FunctionSyntax, FunctionBodyType>
                    (syntax => new FunctionBodyType(this, syntax));

            StopByObjectIds();
        }

        bool IsEndPosition => ViewPosition == Compound.Syntax.EndPosition;

        [DisableDump]
        internal ContextBase Context
            => Compound.Parent.CompoundPositionContext(Compound.Syntax, ViewPosition);

        [DisableDump]
        internal CompoundType Type => TypeCache.Value;

        [DisableDump]
        TypeBase IndexType => Compound.IndexType;

        [DisableDump]
        internal CompoundContext CompoundContext => CompoundContextCache.Value;

        [DisableDump]
        internal Size CompoundViewSize
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
        internal string DumpPrintTextOfType
            => Compound
                .Syntax
                .EndPosition
                .Select(position => Compound.AccessType(ViewPosition, position)?.DumpPrintText ?? "?")
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
        internal bool HasIssues => Issues.Any();

        ValueCache ValueCache.IContainer.Cache { get; } = new ValueCache();

        public string GetCompoundChildDump()
            => Compound.GetCompoundIdentificationDump() + PositionDump();

        string PositionDump()
        {
            if(IsEndPosition)
                return "{}";

            var names =
                Compound
                    .Syntax
                    .NameIndex
                    .Where(item => item.Value == ViewPosition)
                    .Select(item => item.Key)
                    .Stringify("/");

            if(names == "")
                return "@" + ViewPosition;

            return ":" + names;
        }

        public string GetCompoundIdentificationDump()
            => Context.GetContextIdentificationDump();

        protected override string GetNodeDump()
        {
            var result = base.GetNodeDump();
            result += "(" + (Context?.GetContextIdentificationDump() ?? "?") + ")";
            if(HasIssues)
                result += ".Issues[" + Issues.Length + "]";
            return result;
        }

        internal TypeBase FunctionalType(FunctionSyntax syntax) => FunctionBodyTypeCache[syntax];

        internal AccessFeature AccessFeature(int position) => AccessFeaturesCache[position];

        internal TypeBase AccessType(int position)
        {
            var result = Compound.AccessType(ViewPosition, position);
            if(result.IsHollow)
                return result;

            return FieldAccessTypeCache[position];
        }

        internal Result AccessViaPositionExpression(Category category, Result rightResult)
        {
            var position = rightResult
                .Conversion(IndexType)
                .SmartUn<PointerType>()
                .Evaluate(Compound.Root.ExecutionContext)
                .ToInt32();
            return AccessViaObjectPointer(category, position);
        }

        internal Size FieldOffset(int position)
            => Compound.FieldOffsetFromAccessPoint(ViewPosition, position);

        internal Result DumpPrintResultViaObject(Category category)
        {
            if(IsDumpPrintResultViaObjectActive)
                return Root.VoidType.Result(category, () => CodeBase.DumpPrintText("?"));

            IsDumpPrintResultViaObjectActive = true;
            var result = Root.ConcatPrintResult
            (
                category,
                ViewPosition,
                DumpPrintResultViaObject
            );
            IsDumpPrintResultViaObjectActive = false;
            return result;
        }

        Result AccessViaObjectPointer(Category category, int position)
        {
            var resultType = AccessType(position);
            if(resultType.IsHollow)
                return resultType.Result(category);

            return Type.SmartPointer.Mutation(resultType) & category;
        }

        internal Result AccessViaObject(Category category, int position)
        {
            var resultType = AccessType(position);
            if(resultType.IsHollow)
                return resultType.Result(category);

            return resultType.Result(category, Type.ObjectResult);
        }

        internal Result AccessValueViaObject(Category category, int position)
        {
            var resultType = ValueType(position);
            if(resultType.IsHollow)
                return resultType.Result(category);

            return resultType.Result
                (category, c => Type.ObjectResult(c).AddToReference(() => FieldOffset(position)));
        }


        internal Result ReplaceObjectPointerByContext(Result target)
        {
            var reference = Type.SmartPointer.CheckedReference;
            return target.ReplaceAbsolute(reference, ObjectPointerViaContext);
        }

        IConversion ConversionFunction(FunctionSyntax body)
        {
            IConversion result = new ConverterAccess(Function(body, Root.VoidType), Type);
            var source = result.Source;
            (source == Type.Pointer).Assert(source.Dump);
            (source == result.Result(Category.Code).Code.ArgType).Assert();
            return result;
        }

        internal FunctionType Function(FunctionSyntax body, TypeBase argsType)
            => Compound
                .Root
                .FunctionInstance(this, body, argsType.AssertNotNull());

        internal IEnumerable<FunctionType> Functions(FunctionSyntax body)
            => Compound
                .Root
                .FunctionInstances(this, body);

        internal Result ObjectPointerViaContext(Category category)
        {
            if(IsHollow)
                return Type.Result(category);

            return Type.SmartPointer
                .Result
                (
                    category,
                    ObjectPointerViaContext,
                    () => Closures.Create(Compound)
                );
        }

        Result DumpPrintResultViaObject(Category category, int position)
        {
            var trace = ObjectId == -5 && position == 1;
            StartMethodDump(trace, category, position);
            try
            {
                BreakExecution();
                var accessType = ValueType(position).SmartPointer;
                var genericDumpPrintResult = accessType.GenericDumpPrintResult(category);
                Dump("genericDumpPrintResult", genericDumpPrintResult);
                BreakExecution();
                return ReturnMethodDump
                (
                    genericDumpPrintResult.ReplaceAbsolute
                        (accessType.CheckedReference, c => AccessValueViaObject(c, position)));
            }
            finally
            {
                EndMethodDump();
            }
        }

        CodeBase ObjectPointerViaContext()
            => CodeBase.ReferenceCode(Compound).ReferencePlus(CompoundViewSize * -1);

        internal TypeBase ValueType(int position)
        {
            var accessType = Compound
                .AccessType(ViewPosition, position);

            return accessType
                .AssertNotNull()
                .TypeForStructureElement;
        }

        internal IImplementation Find(Definable definable, bool publicOnly)
        {
            var position = Compound.Syntax.Find(definable?.Id, publicOnly);
            return position == null? null : AccessFeature(position.Value);
        }

        internal Result AtTokenResult(Category category, Result rightResult)
            => AccessViaPositionExpression(category, rightResult)
                .ReplaceArg(ObjectPointerViaContext);

        internal Result ContextOperatorResult(Category category)
            => ContextReferenceType.Result(category, ContextOperatorCode);

        CodeBase ContextOperatorCode()
            => IsHollow
                ? CodeBase.Void
                : CodeBase
                    .ReferenceCode(Type.ForcedReference)
                    .ReferencePlus(CompoundViewSize);
    }
}