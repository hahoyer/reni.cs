using System.Collections.Generic;
using Bnf.Forms;
using hw.DebugFormatter;

namespace Bnf
{
    static class Extension
    {
        public static IForm Checked<T>(this IForm form, Syntax parent)
            where T : class
        {
            if((IForm) (form as T) != null)
                return (IForm) (T) form;


            return new Error<T>(parent, form);
        }

        public static void Add<TListItem, TItemForm>(this List<TItemForm> result, IForm form)
        where TListItem: IListForm<TItemForm>
        {
            switch(form)
            {
                case null: return;
                case TListItem listForm:
                    result.AddRange(listForm.Data);
                    return;
                case TItemForm item:
                    result.Add(item);
                    return;
            }

            Dumpable.NotImplementedFunction(result, form);
        }
    }

    interface IListForm<out T>
    {
        T[] Data {get;}
    }
}