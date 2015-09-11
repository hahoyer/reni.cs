using System;
using System.Collections.Generic;
using hw.Helper;
using System.Linq;
using hw.Debug;
using Reni.Basics;
using Reni.Code.ReplaceVisitor;

namespace Reni.Code
{
    sealed class RemoveLocalReferences : Base
    {
        CodeBase Body { get; }
        CodeBase Copier { get; }

        sealed class Counter : Base
        {
            readonly Dictionary<LocalReference, int> _references =
                new Dictionary<LocalReference, int>();
            public Counter(CodeBase body) { body.Visit(this); }

            internal override CodeBase LocalReference(LocalReference visitedObject)
            {
                if(_references.ContainsKey(visitedObject))
                    _references[visitedObject]++;
                else
                    _references.Add(visitedObject, 1);
                visitedObject.ValueCode.Visit(this);
                return null;
            }

            internal LocalReference[] SingleReferences
                => _references
                    .Where(r => r.Value == 1)
                    .Select(r => r.Key)
                    .ToArray();

            internal LocalReference[] References => _references.Keys.ToArray();
        }

        sealed class Reducer : Base
        {
            readonly LocalReference[] _references;
            readonly FunctionCache<LocalReference, LocalReference> _map;

            public Reducer(LocalReference[] references, CodeBase body)
            {
                _references = references;
                _map = new FunctionCache<LocalReference, LocalReference>(GetReplacementsForCache);
                NewBody = GetNewBody(body);
            }

            CodeBase GetNewBody(CodeBase body)
                => _references.Any() ? (body.Visit(this) ?? body) : body;

            public CodeBase NewBody { get; }

            internal override CodeBase LocalReference(LocalReference visitedObject)
                => _references.Contains(visitedObject) ? _map[visitedObject] : null;

            LocalReference GetReplacementsForCache(LocalReference reference)
            {
                var valueCode = reference.ValueCode;
                return (valueCode.Visit(this) ?? valueCode)
                    .LocalReference(reference.ValueType, reference.DestructorCode, true);
            }
        }

        sealed class FinalReplacer : Base
        {
            readonly Size _offset;
            readonly LocalReference _target;

            FinalReplacer(Size offset, LocalReference target)
            {
                _offset = offset;
                _target = target;
            }

            public FinalReplacer(LocalReference target)
                : this(Size.Zero, target) { }

            internal override CodeBase LocalReference(LocalReference visitedObject)
                => visitedObject != _target
                    ? null
                    : CodeBase
                        .TopRef()
                        .ReferencePlus(_offset);


            protected override Visitor<CodeBase,FiberItem> After(Size size)
                => new FinalReplacer(_offset + size,_target);
        }

        static int _nextObjectId;

        readonly ValueCache<CodeBase> _reducedBodyCache;
        readonly ValueCache<LocalReference[]> _referencesCache;

        public RemoveLocalReferences(CodeBase body, CodeBase copier)
            : base(_nextObjectId++)
        {
            Body = body;
            Copier = copier;
            _referencesCache = new ValueCache<LocalReference[]>(GetReferencesForCache);
            _reducedBodyCache = new ValueCache<CodeBase>(GetReducedBodyForCache);
        }

        [DisableDump]
        internal CodeBase NewBody
        {
            get
            {
                if (!References.Any())
                    return ReducedBody;

                var trace = ObjectId < 0;
                StartMethodDump(trace);
                try
                {
                    BreakExecution();

                    Tracer.Assert(!ReducedBody.HasArg, ReducedBody.Dump);

                    Dump(nameof(ReducedBody), ReducedBody);
                    Dump(nameof(References), References);

                    var body = ReducedBody;
                    var initialSize = Size.Zero;
                    foreach(var reference in References)
                    {
                        var initialCode = reference.AlignedValueCode;
                        initialSize += initialCode.Size;
                        Dump(nameof(initialCode), initialCode);

                        var replacedBody = body.Visit(new FinalReplacer(reference)) ?? body;
                        Dump(nameof(replacedBody), replacedBody);
                        body = initialCode + replacedBody;
                        Dump(nameof(body), body);
                        BreakExecution();
                    }


                    var result = body.LocalBlockEnd(Copier, initialSize);
                    return ReturnMethodDump(result);
                }
                finally
                {
                    EndMethodDump();
                }
            }
        }

        LocalReference[] References => _referencesCache.Value;
        CodeBase ReducedBody => _reducedBodyCache.Value;

        CodeBase GetReducedBodyForCache()
            => new Reducer(new Counter(Body).SingleReferences, Body).NewBody;

        LocalReference[] GetReferencesForCache()
            => new Counter(ReducedBody)
                .References;
    }
}