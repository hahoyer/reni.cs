using System.Collections.Generic;

namespace Reni
{
    internal abstract class SearchTrial : ReniObject
    {
        private SearchTrial(string caller)
        {
            _caller = caller;
        }

        private readonly string _caller;

        public override string DumpData()
        {
            return "Called at position:\n\t" + _caller;
        }

        public static SearchTrial Create<TTarget>(TTarget target, string caller) where TTarget : IDumpShortProvider
        {
            return new Based<TTarget>(target, caller);
        }

        public static SearchTrial Create(string caller)
        {
            return new Untyped(caller);
        }

        public static SearchTrial SubTrial<TTarget>(SearchTrial subResult, TTarget target, string caller) where TTarget : IDumpShortProvider
        {
            return new Sub<TTarget>(subResult, target, caller);
        }

        public static SearchTrial AlternativeTrial(SearchTrial failedTrial, SearchTrial alternativeResult, string caller)
        {
            return failedTrial.AddAlternative(alternativeResult, caller);
        }

        protected virtual SearchTrial AddAlternative(SearchTrial alternativeResult, string caller)
        {
            return new Alternatives(this, alternativeResult, caller);
        }

        private sealed class Alternatives : SearchTrial
        {
            public Alternatives(SearchTrial failedTrial, SearchTrial alternativeResult, string caller) : base(caller)
            {
                _trials = new[] {failedTrial, alternativeResult};
            }

            private Alternatives(ICollection<SearchTrial> failedTrials, SearchTrial alternativeResult, string caller)
                : base(caller)
            {
                _trials = new SearchTrial[failedTrials.Count + 1];
                var i = 0;
                foreach(var failedTrial in failedTrials)
                    _trials[i++] = failedTrial;
                _trials[i] = alternativeResult;
            }

            private readonly SearchTrial[] _trials;

            protected override SearchTrial AddAlternative(SearchTrial alternativeResult, string caller)
            {
                return new Alternatives(_trials, alternativeResult, _caller);
            }

            public override string DumpData()
            {
                var result = base.DumpData() + "\n";
                var length = _trials.Length;
                for(var i = 0; i < length; i++)
                {
                    var trial = _trials[i];
                    result += i+". trial of "+length+"\n" + trial.Dump();
                }
                return result;
            }
        }

        private sealed class Based<TTargetType> : SearchTrial where TTargetType : IDumpShortProvider
        {
            internal Based(TTargetType target, string caller)
                : base(caller)
            {
                _target = target;
            }

            private readonly TTargetType _target;

            public override string DumpData()
            {
                return base.DumpData() + "\nTarget=" + _target.DumpShort();
            }
        }

        private sealed class Sub<TTargetType> : SearchTrial where TTargetType : IDumpShortProvider
        {
            public Sub(SearchTrial subResult, TTargetType target, string caller)
                : base(caller)
            {
                _subResult = subResult;
                _target = target;
            }

            private readonly TTargetType _target;
            private readonly SearchTrial _subResult;

            public override string DumpData()
            {
                return base.DumpData() + "\nTarget=" + _target.DumpShort() + "\nSubResult=" + _subResult.Dump();
            }
        }

        private sealed class Untyped : SearchTrial
        {
            internal Untyped(string caller) : base(caller) {}
        }
    }

    internal interface IDumpShortProvider
    {
        string DumpShort();
    }
}