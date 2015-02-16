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
        public CodeBase Body { get; set; }
        public CodeBase Copier { get; set; }

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
            readonly LocalReference[] _references;
            readonly Size _offset;

            public FinalReplacer(LocalReference[] references, Size offset)
            {
                _references = references;
                _offset = offset;
            }
            public FinalReplacer(LocalReference[] references)
                : this(references, Size.Zero) { }

            internal override CodeBase LocalReference(LocalReference visitedObject)
            {
                return CodeBase
                    .TopRef()
                    .ReferencePlus(_offset + Offset(visitedObject));
            }

            Size Offset(LocalReference visitedObject)
            {
                var index = _references
                    .IndexWhere(reference => reference == visitedObject)
                    .AssertValue();
                return _references
                    .Skip(index+1)
                    .Select(reference => reference.AlignedValueCode.Size)
                    .Aggregate()
                    ?? Size.Zero;
            }

            protected override Visitor<CodeBase> After(Size size)
                => new FinalReplacer(_references, _offset + size);
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
                if(!References.Any())
                    return ReducedBody;

                Tracer.Assert(!ReducedBody.HasArg, ReducedBody.Dump);

                var trace = ObjectId == -2;
                StartMethodDump(trace);
                try
                {
                    Dump(nameof(ReducedBody), ReducedBody);
                    Dump(nameof(References), References);

                    var initialCode = References
                        .Select(reference => reference.AlignedValueCode)
                        .Aggregate();
                    Dump(nameof(initialCode), initialCode);

                    var bodyCode = ReducedBody.Visit(new FinalReplacer(References)) ?? ReducedBody;
                    Dump(nameof(bodyCode), bodyCode);

                    BreakExecution();

                    var result = (initialCode + bodyCode).LocalBlockEnd(Copier, Body.Size);
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

        LocalReference[] GetReferencesForCache() => new Counter(ReducedBody).References;
    }
}