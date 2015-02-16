using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Helper;
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

        internal CodeBase NewBody
        {
            get
            {
                if(!References.Any())
                    return ReducedBody;

                Tracer.Assert(!ReducedBody.HasArg, ReducedBody.Dump);

                var trace = ObjectId > -2;
                StartMethodDump(trace);
                try
                {
                    Dump(nameof(ReducedBody), ReducedBody);
                    Dump(nameof(References), References);
                    BreakExecution();
                    throw new NotImplementedException();

                    return ReturnMethodDump(Body);
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