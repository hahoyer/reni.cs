using System;
using System.Collections.Generic;
using System.Linq;
using Bnf.Forms;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;

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

        public static bool IsBelongingTo<T>(this Type childType, Type factoryType)
        {
            return childType.Is<T>() &&
                   !childType.IsAbstract &&
                   childType
                       .GetAttributes<BelongsToAttribute>(true)
                       .Any(attr => factoryType.Is(attr.TokenFactory));
        }
    }

    interface IListForm<out T>
    {
        T[] Data {get;}
    }
}