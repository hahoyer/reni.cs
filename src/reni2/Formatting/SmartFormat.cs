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
    sealed class SmartFormat : DumpableObject
    {
        readonly SourcePart _targetPart;
        readonly IConfiguration _configuration;

        SmartFormat(SourcePart targetPart, IConfiguration configuration)
        {
            _targetPart = targetPart;
            _configuration = configuration;
        }

        internal interface IConfiguration
        {
            string StartGap(WhiteSpaceToken[] rightWhiteSpaces, ITokenClass right);
            string Gap(ITokenClass left, ITokenClass rightTokenClass);
            string Gap(ITokenClass left, WhiteSpaceToken[] rightWhiteSpaces, ITokenClass right);
        }

        internal static string Reformat(SourceSyntax target, SourcePart targetPart)
            =>
                new SmartFormat(targetPart, new SmartConfiguration()).Reformat
                    (null, target, null, null);

        string Reformat
            (
            ITokenClass leftTokenClass,
            SourceSyntax target,
            WhiteSpaceToken[] rightWhiteSpaces,
            ITokenClass rightTokenClass)
        {
            if(target == null)
                return Gap(leftTokenClass, rightWhiteSpaces, rightTokenClass);

            var leftResult = Reformat
                (leftTokenClass, target.Left, target.Token.PrecededWith, target.TokenClass);
            var rightResult = Reformat
                (target.TokenClass, target.Right, rightWhiteSpaces, rightTokenClass);

            return leftResult + target.Token.Characters.Id + rightResult;
        }

        string Gap
            (
            ITokenClass leftTokenClass,
            WhiteSpaceToken[] rightWhiteSpaces,
            ITokenClass rightTokenClass)
        {
            if(leftTokenClass == null)
                return _configuration.StartGap(rightWhiteSpaces, rightTokenClass);

            if(rightTokenClass == null)
            {
                Tracer.Assert(rightWhiteSpaces == null);
                return "";
            }

            return !rightWhiteSpaces.Any()
                ? _configuration.Gap(leftTokenClass, rightTokenClass)
                : _configuration.Gap(leftTokenClass, rightWhiteSpaces, rightTokenClass);
        }

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
    }
}