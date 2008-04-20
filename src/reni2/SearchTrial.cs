using System.Collections.Generic;

namespace Reni
{
    internal abstract class SearchTrial : ReniObject
    {
        private SearchTrial(string caller)
        {
            Caller = caller;
        }

        public string Caller { get; private set; }

        public override string DumpData()
        {
            return "Caller:\n" + Caller;
        }

        public static SearchTrial Create<Target>(Target target, string caller) where Target : IDumpShortProvider
        {
            return new Based<Target>(target, caller);
        }

        public static SearchTrial Create(string caller)
        {
            return new Untyped(caller);
        }

        public static SearchTrial SubTrial<Target>(SearchTrial subResult, Target target, string caller) where Target : IDumpShortProvider
        {
            return new Sub<Target>(subResult, target, caller);
        }

        public static SearchTrial AlternativeTrial(SearchTrial failedTrial, SearchTrial alternativeResult, string caller)
        {
            return failedTrial.AddAlternative(alternativeResult, caller);
        }

        internal virtual SearchTrial AddAlternative(SearchTrial alternativeResult, string caller)
        {
            return new Alternatives(this, alternativeResult, caller);
        }

        internal sealed class Alternatives : SearchTrial
        {
            public Alternatives(SearchTrial failedTrial, SearchTrial alternativeResult, string caller) : base(caller)
            {
                Trials = new[] {failedTrial, alternativeResult};
            }

            private Alternatives(ICollection<SearchTrial> failedTrials, SearchTrial alternativeResult, string caller)
                : base(caller)
            {
                Trials = new SearchTrial[failedTrials.Count + 1];
                var i = 0;
                foreach(var failedTrial in failedTrials)
                    Trials[i++] = failedTrial;
                Trials[i] = alternativeResult;
            }

            internal SearchTrial[] Trials { get; private set; }

            internal override SearchTrial AddAlternative(SearchTrial alternativeResult, string caller)
            {
                return new Alternatives(Trials, alternativeResult, Caller);
            }

            public override string DumpData()
            {
                var result = base.DumpData() + "\nTrials:";
                foreach(var trial in Trials)
                    result += "\n" + trial.Dump();
                return result;
            }
        }

        internal sealed class Based<TargetType> : SearchTrial where TargetType : IDumpShortProvider
        {
            internal Based(TargetType target, string caller)
                : base(caller)
            {
                Target = target;
            }

            public TargetType Target { get; private set; }

            public override string DumpData()
            {
                return base.DumpData() + "\nTarget=" + Target.DumpShort();
            }
        }

        internal sealed class Sub<TargetType> : SearchTrial where TargetType : IDumpShortProvider
        {
            public Sub(SearchTrial subResult, TargetType target, string caller)
                : base(caller)
            {
                SubResult = subResult;
                Target = target;
            }

            public TargetType Target { get; private set; }
            public SearchTrial SubResult { get; private set; }

            public override string DumpData()
            {
                return base.DumpData() + "\nTarget=" + Target.DumpShort() + "\nSubResult=" + SubResult.Dump();
            }
        }

        internal sealed class Untyped : SearchTrial
        {
            internal Untyped(string caller) : base(caller) {}
        }
    }

    internal interface IDumpShortProvider
    {
        string DumpShort();
    }
}