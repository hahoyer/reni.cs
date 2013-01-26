﻿#region Copyright (C) 2012

//     Project Reni2
//     Copyright (C) 2011 - 2012 Harald Hoyer
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

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;

namespace Reni.Parser
{
    abstract class TokenFactory<TTokenClass> : ReniObject, ITokenFactory
        where TTokenClass : class, ITokenClass
    {
        readonly PrioTable _prioTable;
        readonly SimpleCache<DictionaryEx<string, TTokenClass>> _tokenClasses;
        readonly SimpleCache<TTokenClass> _number;
        readonly SimpleCache<TTokenClass> _text;
        readonly SimpleCache<TTokenClass> _beginOfText;
        readonly SimpleCache<TTokenClass> _endOfText;

        internal TokenFactory(PrioTable prioTable)
        {
            _prioTable = prioTable;
            _endOfText = new SimpleCache<TTokenClass>(InternalGetEndOfText);
            _beginOfText = new SimpleCache<TTokenClass>(InternalGetBeginOfText);
            _number = new SimpleCache<TTokenClass>(InternalGetNumber);
            _tokenClasses = new SimpleCache<DictionaryEx<string, TTokenClass>>(InternalGetTokenClasses);
            _text = new SimpleCache<TTokenClass>(InternalGetText);
        }
        TTokenClass InternalGetBeginOfText()
        {
            var result = GetBeginOfText();
            result.Name = PrioTable.BeginOfText;
            return result;
        }
        TTokenClass InternalGetEndOfText()
        {
            var result = GetEndOfText();
            result.Name = PrioTable.EndOfText;
            return result;
        }

        DictionaryEx<string, TTokenClass> InternalGetTokenClasses()
        {
            var result = GetTokenClasses();
            foreach(var pair in result)
                pair.Value.Name = pair.Key;
            return result;
        }

        TTokenClass InternalGetNumber()
        {
            var result = GetNumber();
            result.Name = "<number>";
            return result;
        }

        TTokenClass InternalGetText()
        {
            var result = GetText();
            result.Name = "<Text>";
            return result;
        }

        PrioTable ITokenFactory.PrioTable { get { return _prioTable; } }

        ITokenClass ITokenFactory.TokenClass(string name)
        {
            TTokenClass result;
            if(TokenClasses.TryGetValue(name, out result))
                return result;
            result = GetNewToken(name);
            result.Name = name;
            TokenClasses.Add(name, result);
            return result;
        }

        ITokenClass ITokenFactory.Number { get { return _number.Value; } }
        ITokenClass ITokenFactory.Text { get { return _text.Value; } }
        ITokenClass ITokenFactory.BeginOfText { get { return _beginOfText.Value; } }
        ITokenClass ITokenFactory.EndOfText { get { return _endOfText.Value; } }

        protected abstract TTokenClass GetSyntaxError(string message);
        protected abstract DictionaryEx<string, TTokenClass> GetTokenClasses();

        protected virtual TTokenClass GetEndOfText() { return GetSyntaxError("unexpected end of text".Quote()); }
        protected virtual TTokenClass GetBeginOfText() { return GetSyntaxError("unexpected begin of text".Quote()); }
        protected virtual TTokenClass GetNewToken(string name) { return GetSyntaxError("invalid symbol: " + name.Quote()); }
        protected virtual TTokenClass GetNumber() { return GetSyntaxError("unexpected number"); }
        protected virtual TTokenClass GetText() { return GetSyntaxError("unexpected string"); }

        Dictionary<string, TTokenClass> TokenClasses { get { return _tokenClasses.Value; } }
        protected ITokenClass TokenClass(string name) { return ((ITokenFactory) this).TokenClass(name); }
    }
}