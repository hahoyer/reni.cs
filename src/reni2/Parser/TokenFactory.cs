//     Compiler for programming language "Reni"
//     Copyright (C) 2011 Harald Hoyer
// 
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//     
//     Comments, bugs and suggestions to hahoyer at yahoo.de

using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;

namespace Reni.Parser
{
    internal abstract class TokenFactory<TTokenClass> : ReniObject, ITokenFactory
        where TTokenClass : class, ITokenClass 
    {
        private readonly SimpleCache<DictionaryEx<string, TTokenClass>> _tokenClasses;
        private readonly SimpleCache<PrioTable> _prioTable;
        private readonly SimpleCache<TTokenClass> _listClass;
        private readonly SimpleCache<TTokenClass> _numberClass;
        private readonly SimpleCache<TTokenClass> _textClass;
        private readonly DictionaryEx<int, TTokenClass> _leftParenthesis;
        private readonly DictionaryEx<int, TTokenClass> _righParenthesis;

        internal TokenFactory()
        {
            _leftParenthesis = new DictionaryEx<int, TTokenClass>(InternalGetLeftParenthesisClass);
            _righParenthesis = new DictionaryEx<int, TTokenClass>(InternalGetRightParenthesisClass);
            _numberClass = new SimpleCache<TTokenClass>(InternalGetNumberClass);
            _listClass = new SimpleCache<TTokenClass>(InternalGetListClass);
            _prioTable = new SimpleCache<PrioTable>(GetPrioTable);
            _tokenClasses = new SimpleCache<DictionaryEx<string, TTokenClass>>(InternalGetTokenClasses);
            _textClass = new SimpleCache<TTokenClass>(InternalGetTextClass);
        }

        private DictionaryEx<string, TTokenClass> InternalGetTokenClasses()
        {
            DictionaryEx<string, TTokenClass> result = GetTokenClasses();
            foreach(var pair in result)
                pair.Value.Name = pair.Key;
            return result;
        }

        private TTokenClass InternalGetListClass()
        {
            var result = GetListClass();
            result.Name = ",";
            return result;
        }

        private TTokenClass InternalGetNumberClass()
        {
            var result = GetNumberClass();
            result.Name = "<number>";
            return result;
        }

        private TTokenClass InternalGetTextClass()
        {
            var result = GetTextClass();
            result.Name = "<Text>";
            return result;
            ;
        }

        private TTokenClass InternalGetRightParenthesisClass(int i)
        {
            var result = GetRightParenthesisClass(i);
            result.Name = i == 0 ? "<end>" : " }])".Substring(i, 1);
            return result;
        }

        private TTokenClass InternalGetLeftParenthesisClass(int i)
        {
            var result = GetLeftParenthesisClass(i);
            result.Name = i == 0 ? "<frame>" : " {[(".Substring(i, 1);
            return result;
        }

        ParserInst ITokenFactory.Parser { get { return new ParserInst(new Scanner(), this); } }
        PrioTable ITokenFactory.PrioTable { get { return _prioTable.Value; } }

        ITokenClass ITokenFactory.TokenClass(string name)
        {
            TTokenClass result;
            if(TokenClasses.TryGetValue(name, out result))
                return result;
            result = GetNewTokenClass(name);
            result.Name = name;
            TokenClasses.Add(name, result);
            return result;
        }

        ITokenClass ITokenFactory.ListClass { get { return _listClass.Value; } }
        ITokenClass ITokenFactory.NumberClass { get { return _numberClass.Value; } }
        ITokenClass ITokenFactory.TextClass { get { return _textClass.Value; } }
        ITokenClass ITokenFactory.RightParenthesisClass(int level) { return _righParenthesis.Value(level); }
        ITokenClass ITokenFactory.LeftParenthesisClass(int level) { return _leftParenthesis.Value(level); }

        protected abstract TTokenClass GetSyntaxError(string message);
        protected abstract PrioTable GetPrioTable();
        protected abstract DictionaryEx<string, TTokenClass> GetTokenClasses();

        protected virtual TTokenClass GetNewTokenClass(string name) { return GetSyntaxError("invalid symbol: " + name.Quote()); }
        protected virtual TTokenClass GetListClass() { return GetSyntaxError("unexpected list token"); }
        protected virtual TTokenClass GetRightParenthesisClass(int level) { return GetSyntaxError("unexpected parenthesis"); }
        protected virtual TTokenClass GetLeftParenthesisClass(int level) { return GetSyntaxError("unexpected parenthesis"); }
        protected virtual TTokenClass GetNumberClass() { return GetSyntaxError("unexpected number"); }
        protected virtual TTokenClass GetTextClass() { return GetSyntaxError("unexpected string"); }

        private Dictionary<string, TTokenClass> TokenClasses { get { return _tokenClasses.Value; } }
        protected ITokenClass TokenClass(string name) { return ((ITokenFactory) this).TokenClass(name); }
    }
}