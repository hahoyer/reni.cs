using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using Stx.CodeItems;
using Stx.Contexts;
using Stx.DataTypes;
using Stx.Features;

namespace Stx.Forms
{
    interface IForm
    {
        Result GetResult(Context context);
    }


    abstract class FormBase : DumpableObject, IForm
    {
        public static IForm CreateUserSymbol(Syntax parent, string name, IForm index)
            => index == null
                ? (IForm) new UserSymbolForm(parent, name)
                : new UserArraySymbolForm(parent, name, index);
            ;

        readonly FunctionCache<Context, Result> ResultCache;
        protected FormBase() => ResultCache = new FunctionCache<Context, Result>(GetResult);
        Result IForm.GetResult(Context context) => ResultCache[context];
        protected abstract Result GetResult(Context context);
        public static IForm CreateReassign(Syntax parent, IForm destination, IForm source)=>new ReassignForm(parent, destination, source);
    }

    sealed class ReassignForm : FormBase
    {
        readonly Syntax Parent;
        readonly IForm Destination;
        readonly IForm Source;

        public ReassignForm(Syntax parent, IForm destination, IForm source)
        {
            Parent = parent;
            Destination = destination;
            Source = source;
        }

        static Result ValidateDestination
            (Context context, Syntax syntax, SourcePart position, DataType inferredDataType)
            => syntax == null
                ? IssueId.ReassignDestinationMissing.At(position)
                : syntax.GetResult(context.ReassignDestination(inferredDataType));

        static Result ValidateValue(Context context, Syntax syntax, SourcePart position)
            => syntax == null
                ? IssueId.ReassignValueMissing.At(position)
                : syntax.GetResult(context.ReassignValue);


        protected override Result GetResult(Context context)
        {
            NotImplementedMethod(context);
            return null;
        
            
            var value = ValidateValue(context, right, token.Characters);
            var destination = ValidateDestination(context, left, token.Characters, value.DataType);

            return
                new Result
                (
                    token.Characters,
                    DataType.Void,
                    getCodeItems: () =>
                        CodeItem
                            .Combine
                            (
                                destination.CodeItems,
                                CodeItem.CreateSourceHint(token),
                                value.CodeItems,
                                CodeItem.CreateReassign(value.ByteSize)
                            )
                            .ToArray()
                );
        }
    }

    sealed class UserArraySymbolForm : FormBase
    {
        readonly IForm Index;
        readonly string Name;
        readonly Syntax Parent;

        public UserArraySymbolForm(Syntax parent, string name, IForm index)
        {
            Parent = parent;
            Name = name;
            Index = index;
        }

        protected override Result GetResult(Context context)
        {
            NotImplementedMethod(context);
            return null;

            var value = Index.GetResult(context.Subscription);
            var token = Parent.Token;
            var variable = context.Access(this, token, value?.DataType);
            return variable.Subscription(token.Characters, value);
        }
    }

    sealed class UserSymbolForm : FormBase
    {
        readonly string Name;
        readonly Syntax Parent;

        public UserSymbolForm(Syntax parent, string name)
        {
            Parent = parent;
            Name = name;
        }

        protected override Result GetResult(Context context)
        {
            NotImplementedMethod(context);
            return null;

            var token = Parent.Token;
            return context.Access(this, token, null);
        }
    }
}