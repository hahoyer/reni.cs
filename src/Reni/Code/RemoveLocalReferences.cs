using hw.DebugFormatter;
using hw.Helper;
using Reni.Basics;
using Reni.Code.ReplaceVisitor;

namespace Reni.Code;

sealed class RemoveLocalReferences : Base
{
    sealed class Counter : Base
    {
        readonly Dictionary<LocalReference, int> ReferencesCounts = new();

        internal LocalReference[] SingleReferences
            => ReferencesCounts
                .Where(r => r.Value == 1)
                .Select(r => r.Key)
                .ToArray();

        internal LocalReference[] References => ReferencesCounts.Keys.ToArray();

        public Counter(CodeBase body) => body.Visit(this);

        internal override CodeBase LocalReference(LocalReference visitedObject)
        {
            if(ReferencesCounts.ContainsKey(visitedObject))
                ReferencesCounts[visitedObject]++;
            else
                ReferencesCounts.Add(visitedObject, 1);
            visitedObject.ValueCode.Visit(this);
            return null;
        }
    }

    sealed class Reducer : Base
    {
        public CodeBase NewBody { get; }
        readonly FunctionCache<LocalReference, LocalReference> Map;
        readonly LocalReference[] References;

        public Reducer(LocalReference[] references, CodeBase body)
        {
            References = references;
            Map = new(GetReplacementsForCache);
            NewBody = GetNewBody(body);
        }

        internal override CodeBase LocalReference(LocalReference visitedObject)
            => References.Contains(visitedObject)? Map[visitedObject] : null;

        CodeBase GetNewBody(CodeBase body)
            => References.Any()? body.Visit(this) ?? body : body;

        LocalReference GetReplacementsForCache(LocalReference reference)
        {
            var valueCode = reference.ValueCode;
            return (valueCode.Visit(this) ?? valueCode)
                .GetLocalReference(reference.ValueType, true);
        }
    }

    sealed class FinalReplacer : Base
    {
        readonly Size Offset;
        readonly LocalReference Target;

        public FinalReplacer(LocalReference target)
            : this(Size.Zero, target) { }

        FinalReplacer(Size offset, LocalReference target)
        {
            Offset = offset;
            Target = target;
        }

        internal override CodeBase LocalReference(LocalReference visitedObject)
            => visitedObject != Target
                ? null
                : CodeBase
                    .GetTopRef()
                    .GetReferenceWithOffset(Offset);


        protected override Visitor<CodeBase, FiberItem> After(Size size)
            => new FinalReplacer(Offset + size, Target);
    }

    static int NextObjectId;
    readonly ValueCache<CodeBase> ReducedBodyCache;
    readonly ValueCache<LocalReference[]> ReferencesCache;

    CodeBase Body { get; }
    CodeBase Copier { get; }

    [DisableDump]
    internal CodeBase NewBody
    {
        get
        {
            if(!References.Any())
                return ReducedBody;

            var trace = ObjectId == -10;
            StartMethodDump(trace);
            try
            {
                BreakExecution();

                (!ReducedBody.HasArgument).Assert(ReducedBody.Dump);

                Dump(nameof(ReducedBody), ReducedBody);
                Dump(nameof(References), References);

                var body = ReducedBody;
                var initialSize = Size.Zero;

                var cleanup = new Result(Category.Code | Category.Closures, getIsHollow: () => true);

                foreach(var reference in References)
                {
                    Dump(nameof(reference), reference);
                    var initialCode = reference.AlignedValueCode;
                    initialSize += initialCode.Size;
                    Dump(nameof(initialCode), initialCode);

                    var replacedBody = body.Visit(new FinalReplacer(reference)) ?? body;
                    Dump(nameof(replacedBody), replacedBody);
                    body = initialCode + replacedBody;
                    Dump(nameof(body), body);
                    BreakExecution();

                    var cleanup1 = reference
                        .ValueType
                        .GetCleanup(Category.Code | Category.Closures)
                        .ReplaceAbsolute(reference.ValueType.ForcedPointer, CodeBase.GetTopRef, Closures.GetVoid);
                    cleanup = cleanup1 + cleanup;
                    Dump(nameof(cleanup), cleanup);
                    BreakExecution();
                }

                var result = (body + cleanup.Code).GetLocalBlockEnd(Copier, initialSize);
                return ReturnMethodDump(result);
            }
            finally
            {
                EndMethodDump();
            }
        }
    }

    LocalReference[] References => ReferencesCache.Value;
    CodeBase ReducedBody => ReducedBodyCache.Value;

    public RemoveLocalReferences(CodeBase body, CodeBase copier)
        : base(NextObjectId++)
    {
        Body = body;
        Copier = copier;
        ReferencesCache = new(GetReferencesForCache);
        ReducedBodyCache = new(GetReducedBodyForCache);
    }

    CodeBase GetReducedBodyForCache()
        => new Reducer(new Counter(Body).SingleReferences, Body).NewBody;

    LocalReference[] GetReferencesForCache()
        => new Counter(ReducedBody)
            .References;
}