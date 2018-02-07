using Bnf.DataTypes;
using Bnf.Features;
using Bnf.TokenClasses;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using hw.Scanner;

namespace Bnf.Contexts
{
    interface IExtension {}

    class Context : DumpableObject
    {
        public static readonly Context Root = new Context(null, "root");

        [EnableDumpExcept(null)]
        public readonly string Id;

        [EnableDumpExcept(null)]
        public Context Parent;

        readonly FunctionCache<DataType, Context> ReassignDestinationCache;
        readonly FunctionCache<string, WithName> WithVariableCache;

        Context InBracketsCache;

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
        public Context ReassignValue => ReassignValueCache ?? (ReassignValueCache = new ReassignValue(this));

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
            var e = dataType.AsExtension;
            if(e == null)
                return this;

            NotImplementedMethod(dataType);
            return null;
        }

        protected virtual Result Access(UserSymbol name, IToken token, DataType subsctiptionDataType)
        {
            if(Parent != null)
                return AccessCorrection(Parent.Access(name, token, subsctiptionDataType));

            NotImplementedMethod(name, token, subsctiptionDataType);
            return null;
        }

        protected virtual Result AccessCorrection(Result result) => result;

        public Context WithVariable(string name, DataType dataType)
            => WithVariableCache[name].WithType(dataType);

        public virtual Result UserSymbol(SourcePart position, string name)
        {
            NotImplementedMethod(position, name);
            return null;
        }

        protected virtual Result UserSymbolReference(SourcePart position, string name)
        {
            if(Parent != null)
                return Parent.UserSymbolReference(position, name);

            NotImplementedMethod(position, name);
            return null;
        }
    }

    sealed class Subscription : Context
    {
        public Subscription(Context parent)
            : base(parent) {}

        protected override Result AccessCorrection(Result result) => result.Rereference;
    }

    sealed class ReassignValue : Context
    {
        public ReassignValue(Context parent)
            : base(parent) {}

        public override Result UserSymbol(SourcePart position, string name)
            => UserSymbolReference(position, name).Dereference;
    }

    sealed class ReassignDestination : Context
    {
        readonly DataType TargetType;

        public ReassignDestination(Context parent, DataType targetType)
            : base(parent) => TargetType = targetType;

        public override Result UserSymbol(SourcePart position, string name)
            => UserSymbolReference(position, name);
    }
}