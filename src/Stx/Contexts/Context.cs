using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using Stx.DataTypes;
using Stx.Features;
using Stx.TokenClasses;

namespace Stx.Contexts
{
    class Context : DumpableObject
    {
        public static readonly Context Root = new Context(null, "root");

        [EnableDumpExcept(null)]
        public readonly string Id;

        readonly FunctionCache<DataType, Context> ReassignDestinationCache;
        readonly FunctionCache<string, WithName> WithVariableCache;

        Context InBracketsCache;

        [EnableDumpExcept(null)]
        public Context Parent;

        Context ReassignValueCache;
        Context SubscriptionCache;

        protected Context(Context parent, string id = null)
        {
            Parent = parent;
            Id = id;
            ReassignDestinationCache = new FunctionCache<DataType, Context>(i => new ReassignDestination(this, i));
            WithVariableCache = new FunctionCache<string, WithName>(i => new WithName(this, i));
        }

        [DisableDump]
        public Context Subscription => SubscriptionCache ?? (SubscriptionCache = new Subscription(this));

        [DisableDump]
        public Context InBrackets => InBracketsCache ?? (InBracketsCache = new Context(this, "InBrackets"));

        [DisableDump]
        public Context ReassignValue => ReassignValueCache ?? (ReassignValueCache = new Context(this, "ReassignValue"));

        [DisableDump]
        public virtual DataType InferredDataType
        {
            get
            {
                NotImplementedMethod();
                return null;
            }
        }

        public Context ReassignDestination(DataType dataType)
            => ReassignDestinationCache[dataType];

        public Context Extend(DataType dataType)
        {
            NotImplementedMethod(dataType);
            return null;
        }

        public virtual Result Access(UserSymbol name, IToken token, DataType subsctiptionDataType)
        {
            if(Parent != null)
                return AccessCorrection(Parent.Access(name, token, subsctiptionDataType));

            NotImplementedMethod(name, token, subsctiptionDataType);
            return null;
        }

        protected virtual Result AccessCorrection(Result result) => result;

        public Context WithVariable(string name, DataType dataType)
            => WithVariableCache[name].WithType(dataType);
    }

    sealed class Subscription : Context
    {
        public Subscription(Context parent):base(parent) {}

        protected override Result AccessCorrection(Result result)
        {
            return result.Rereference;
        }
    }

    sealed class ReassignDestination : Context
    {
        public ReassignDestination(Context parent, DataType inferredDataType)
            : base(parent) => InferredDataType = inferredDataType;

        public override DataType InferredDataType {get;}
    }
}