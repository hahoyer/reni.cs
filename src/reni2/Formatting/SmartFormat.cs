using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Helper;
using hw.Parser;
using hw.Scanner;
using Reni.TokenClasses;

namespace Reni.Formatting
{
    static class SmartFormat
    {
        internal interface ITree
        {
            string Format(SourcePart targetPart);
            WhiteSpaceToken[] PrecededWith { get; }
        }

        sealed class Chain : DumpableObject, ITree
        {
            internal sealed class Item : DumpableObject
            {
                internal readonly ITokenClass _tokenClass;
                [EnableDump]
                internal readonly IToken _token;
                [EnableDump]
                internal readonly ITree _item;

                internal Item(ITokenClass tokenClass, IToken token, SourceSyntax item)
                {
                    _item = SmartFormat.Create(item);
                    _token = token;
                    _tokenClass = tokenClass;
                }

                public WhiteSpaceToken[] PrecededWith => _token?.PrecededWith ?? _item.PrecededWith;

                internal string Format(SourcePart targetPart)
                    => TokenFormat(_token, targetPart) +
                        (_item?.Format(targetPart) ?? "");

                string TokenFormat(IToken token, SourcePart targetPart)
                {
                    if(token == null)
                        return "";

                    if(targetPart.Contains(token.Characters))
                        return token.Characters.Id;

                    NotImplementedMethod(token, targetPart);
                    return null;
                }
            }

            internal static Chain Create(SourceSyntax target)
                => new Chain(CreateReverseChainItems(target).Reverse());

            [EnableDump]
            readonly Item[] _items;

            Chain(IEnumerable<Item> items) { _items = items.ToArray(); }

            public string Format(SourcePart targetPart)
                => _items
                    .Select((item, index) => Contact(index) + item.Format(targetPart))
                    .Stringify("");

            WhiteSpaceToken[] ITree.PrecededWith => _items.First().PrecededWith;

            string Contact(int index)
            {
                if(index == 0)
                    return "";

                var formerItem = _items[index - 1];
                var thisItem = _items[index];
                if(formerItem._item == null && !thisItem._token.PrecededWith.Any())
                    return
                        SeparatorType
                            .Get(formerItem._tokenClass, thisItem._tokenClass)
                            .Text;

                NotImplementedMethod(index);
                return null;
            }

            static IEnumerable<Item> CreateReverseChainItems(SourceSyntax target)
            {
                do
                {
                    yield return new Item(target.TokenClass, target.Token, target.Right);
                    target = target.Left;

                    if(target == null)
                        yield break;
                } while(true);
            }
        }

        sealed class List : DumpableObject, ITree
        {
            internal sealed class Item : DumpableObject
            {
                [EnableDump]
                readonly ITree _item;
                [EnableDump]
                readonly IToken _token;

                internal Item(SourceSyntax item, IToken token)
                {
                    _item = SmartFormat.Create(item);
                    _token = token;
                }

                public WhiteSpaceToken[] PrecededWith
                {
                    get
                    {
                        if(_item == null)
                            return new WhiteSpaceToken[0];
                        return _item.PrecededWith;
                    }
                }

                public string Format(SourcePart targetPart)
                    => _item.Format(targetPart) + TokenFormat(_token, targetPart);

                string TokenFormat(IToken token, SourcePart targetPart)
                {
                    if(token == null)
                        return "";

                    if(targetPart.Contains(token.Characters) && !token.PrecededWith.Any())
                        return token.Characters.Id;

                    NotImplementedMethod(token, targetPart);
                    return null;
                }
            }

            internal static List Create(SourceSyntax target)
            {
                var l = target?.TokenClass as TokenClasses.List;
                return l == null ? null : new List(l, CreateListItems(l, target));
            }

            internal ITree Create(Brace.Head head)
                => new BracedList(head, _list, _items);

            static IEnumerable<Item> CreateListItems(TokenClasses.List list, SourceSyntax target)
            {
                do
                {
                    yield return new Item(target.Left, target.Token);
                    target = target.Right;

                    if(target == null)
                        yield break;

                    if(target.TokenClass == list)
                        continue;

                    yield return new Item(target, null);
                    yield break;
                } while(true);
            }

            readonly TokenClasses.List _list;
            [EnableDump]
            readonly Item[] _items;

            List(TokenClasses.List list, IEnumerable<Item> items)
            {
                _list = list;
                _items = items.ToArray();
            }

            WhiteSpaceToken[] ITree.PrecededWith => _items.First().PrecededWith;

            string ITree.Format(SourcePart targetPart)
            {
                NotImplementedMethod(targetPart);
                return null;
            }
        }

        sealed class Brace : DumpableObject, ITree
        {
            internal sealed class Head
            {
                public Head
                    (
                    LeftParenthesis leftParenthesis,
                    IToken leftToken,
                    IToken rightToken,
                    RightParenthesis rightParenthesis)
                {
                    LeftParenthesis = leftParenthesis;
                    LeftToken = leftToken;
                    RightToken = rightToken;
                    RightParenthesis = rightParenthesis;
                }

                LeftParenthesis LeftParenthesis { get; }
                IToken LeftToken { get; }
                IToken RightToken { get; }
                RightParenthesis RightParenthesis { get; }
                public WhiteSpaceToken[] PrecededWith => LeftToken.PrecededWith;
            }

            internal static ITree Create(SourceSyntax target)
            {
                var rightParenthesis = target?.TokenClass as RightParenthesis;

                if(rightParenthesis == null)
                    return null;

                if(target.Right != null)
                    return null;

                var left = target.Left;
                var leftParenthesis = left?.TokenClass as LeftParenthesis;

                if(leftParenthesis?.Level != rightParenthesis.Level || left.Left != null)
                    return null;

                var braceHead
                    = new Head(leftParenthesis, left.Token, target.Token, rightParenthesis);

                return List
                    .Create(left.Right)
                    ?.Create(braceHead)
                    ?? new Brace(braceHead, left.Right);
            }

            readonly Head _head;
            [EnableDump]
            readonly ITree _target;

            Brace(Head head, SourceSyntax target)
            {
                _head = head;
                _target = SmartFormat.Create(target);
            }

            WhiteSpaceToken[] ITree.PrecededWith => _head.PrecededWith;

            string ITree.Format(SourcePart targetPart)
            {
                NotImplementedMethod(targetPart);
                return null;
            }
        }

        sealed class BracedList : DumpableObject, ITree
        {
            readonly Brace.Head _head;
            readonly TokenClasses.List _list;
            [EnableDump]
            readonly List.Item[] _items;

            internal BracedList(Brace.Head head, TokenClasses.List list, List.Item[] items)
            {
                _list = list;
                _head = head;
                _items = items;
            }

            WhiteSpaceToken[] ITree.PrecededWith => _head.PrecededWith;

            string ITree.Format(SourcePart targetPart)
            {
                var items =
                    _items.Select((item, index) => Contact(index) + item.Format(targetPart))
                        .ToArray();


                NotImplementedMethod(targetPart, nameof(items), items);
                return null;
            }

            string Contact(int index)
            {
                var thisItem = _items[index];
                if(!thisItem.PrecededWith.Any())
                    return "";

                NotImplementedMethod(index);
                return null;
            }
        }

        static ITree Create(SourceSyntax target)
            => target == null
                ? null
                : Brace.Create(target)
                    ?? (ITree) List.Create(target)
                        ?? Chain.Create(target);

        internal static string Reformat(SourceSyntax target, SourcePart targetPart)
        {
            return Reformat(null, target, null, null);
        }

        static string Reformat
            (
            ITokenClass leftTokenClass,
            SourceSyntax target,
            WhiteSpaceToken[] rightWhiteSpaces,
            ITokenClass rightTokenClass)
        {
            if(target == null)
            {
                if(leftTokenClass == null || rightTokenClass == null)
                    return "";
                if(rightWhiteSpaces.Any())
                    Dumpable.NotImplementedFunction
                        (
                            leftTokenClass,
                            target,
                            rightWhiteSpaces,
                            rightTokenClass
                        );

                return SeparatorType.Get(leftTokenClass, rightTokenClass).Text;
            }

            var leftResult = Reformat
                (leftTokenClass, target.Left, target.Token.PrecededWith, target.TokenClass);
            var rightResult = Reformat
                (target.TokenClass, target.Right, rightWhiteSpaces, rightTokenClass);

            if(!target.Token.PrecededWith.Any())
                return leftResult + target.Token.Characters.Id + rightResult;

            Dumpable.NotImplementedFunction
                (
                    leftTokenClass,
                    target,
                    rightWhiteSpaces,
                    rightTokenClass,
                    nameof(leftResult),
                    leftResult,
                    nameof(rightResult),
                    rightResult
                );

            return null;
        }

        static IEnumerable<ContactItem> Combine(IReadOnlyList<SourceSyntax> items)
        {
            yield return new ContactItem(null, items[0]);
            for(var i = 1; i < items.Count; i++)
                yield return new ContactItem(items[i - 1].TokenClass, items[i]);
        }

        static IEnumerable<SourceSyntax> Flatten(SourceSyntax target)
        {
            while(true)
            {
                if(target.Left != null)
                    foreach(var result in Flatten(target.Left))
                        yield return result;
                yield return target;
                if(target.Right == null)
                    yield break;
                target = target.Right;
            }
        }
    }

    sealed class ContactItem : DumpableObject
    {
        [EnableDump]
        readonly ITokenClass _leftTokenClass;
        readonly SourceSyntax _right;

        internal ContactItem(ITokenClass leftTokenClass, SourceSyntax right)
        {
            _leftTokenClass = leftTokenClass;
            _right = right;
        }

        [DisableDump]
        internal ISeparatorType SeparatorType
        {
            get
            {
                if(_leftTokenClass == null)
                    return Formatting.SeparatorType.None;
                if(RightTokenClass is List)
                    return Formatting.SeparatorType.Contact;

                return null;
            }
        }

        [EnableDump]
        ITokenClass RightTokenClass => _right.TokenClass;

        internal string Format(SourcePart targetPart)
        {
            NotImplementedMethod(targetPart);
            return null;
        }
    }
}