using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using Stx.Contexts;
using Stx.Features;

namespace Stx.Forms
{
    interface IForm
    {
        Result GetResult(Context context);
    }


    abstract class FormBase : DumpableObject, IForm
    {
        readonly FunctionCache<Context, Result> ResultCache;

        protected FormBase() => ResultCache = new FunctionCache<Context, Result>(GetResult);
        Result IForm.GetResult(Context context) => ResultCache[context];

        protected abstract Result GetResult(Context context);
    }

    interface ISequence
    {
        ISequenceItem[] Items {get;}
    }

    interface ISequenceItem {}

    interface IError {}


    sealed class Sequence : FormBase, ISequence
    {
        readonly ISequenceItem[] Items;
        readonly Syntax Parent;

        public Sequence(Syntax parent, IEnumerable<ISequenceItem> items)
        {
            Parent = parent;
            Items = items.ToArray();
        }

        ISequenceItem[] ISequence.Items => Items;

        protected override Result GetResult(Context context)
        {
            NotImplementedMethod(context);
            return null;
        }
    }

    sealed class Case : FormBase, Case.IForm
    {
        internal interface IForm {}

        internal sealed class Body : FormBase, IBody
        {
            readonly IItems Items;
            readonly Syntax Parent;
            readonly IExpression Value;

            public Body(Syntax parent, IExpression value, IItems items)
            {
                Parent = parent;
                Value = value;
                Items = items;
            }

            IExpression IBody.Value => Value;
            IItems IBody.Items => Items;

            protected override Result GetResult(Context context) => throw new NotImplementedException();
        }

        public interface IBody
        {
            IExpression Value {get;}
            IItems Items {get;}
        }

        internal interface IItems {}

        readonly IItems Items;
        readonly Syntax Parent;

        readonly IExpression Value;

        public Case(Syntax parent, IBody body)
        {
            Parent = parent;
            Value = body.Value;
            Items = body.Items;
        }

        protected override Result GetResult(Context context) => throw new NotImplementedException();
    }

    sealed class ReassignForm : FormBase
    {
        internal interface IDestination {}

        readonly IDestination Destination;
        readonly Syntax Parent;
        readonly IExpression Source;

        public ReassignForm(Syntax parent, IDestination destination, IExpression source)
        {
            Parent = parent;
            Destination = destination;
            Source = source;
        }


        protected override Result GetResult(Context context)
        {
            NotImplementedMethod(context);
            return null;
        }
    }

    interface IExpression {}

    sealed class UserSymbolFormWithIndex : FormBase, ReassignForm.IDestination, IExpression
    {
        readonly IForm Index;
        readonly string Name;
        readonly Syntax Parent;

        public UserSymbolFormWithIndex(Syntax parent, string name, IForm index)
        {
            Parent = parent;
            Name = name;
            Index = index;
        }

        protected override Result GetResult(Context context)
        {
            NotImplementedMethod(context);
            return null;
        }
    }

    sealed class UserSymbolForm : FormBase, ReassignForm.IDestination, IExpression
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
        }
    }

    sealed class Error<T> : IForm, IError
        where T : class
    {
        IForm Form;
        Syntax Parent;

        public Error(Syntax parent, IForm form)
        {
            Parent = parent;
            Form = form;
        }

        Result IForm.GetResult(Context context) => throw new NotImplementedException();

        string Message => "Form is not " + typeof(T).Name;
    }
}