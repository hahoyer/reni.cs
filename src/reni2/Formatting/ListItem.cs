using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Helper;
using hw.Parser;
using Reni.TokenClasses;

namespace Reni.Formatting
{
    sealed class ListItem : DumpableObject
    {
        readonly List _token;
        readonly Item[] _data;

        ListItem(List token, IEnumerable<Item> data)
        {
            _token = token;
            _data = data.ToArray();
        }

        public static ListItem CheckedCreate(SourceSyntax target)
        {
            var list = target.TokenClass as List;
            return list == null ? null : new ListItem(list, Rearrange(target, list));
        }

        sealed class Item : DumpableObject
        {
            internal readonly SourceSyntax Left;
            internal readonly IToken Token;

            internal Item(SourceSyntax left, IToken token)
            {
                Left = left;
                Token = token;
            }
        }

        static IEnumerable<Item> Rearrange(SourceSyntax target, List list)
        {
            do
            {
                yield return new Item(target.Left, target.Token);

                target = target.Right;

                if(target == null)
                    yield break;

                if(target.TokenClass != list)
                {
                    yield return new Item(target, null);
                    yield break;
                }
            } while(true);
        }

        internal string Reformat(DefaultFormat defaultFormat)
        {
            var items = _data.Select(item => ListLine(defaultFormat, item.Left, item.Token));
            var separator = DefaultFormat.ListSeparator(_token.Level);
            return DefaultFormat.Grouped(items, separator.Text)
                .Stringify(separator.Text);
        }

        string ListLine
            (DefaultFormat defaultFormat, SourceSyntax target, IToken token)
        {
            var text = target?.Reformat(defaultFormat);
            return text + defaultFormat.Separator(target?.RightMostTokenClass, _token).Text +
                (defaultFormat.Format(token) ?? "");
        }
    }
}