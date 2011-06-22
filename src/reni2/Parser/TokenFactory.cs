using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;

namespace Reni.Parser
{
    internal abstract class TokenFactory<TTokenClass> : ReniObject, ITokenFactory
        where TTokenClass : ITokenClass
    {
        private readonly SimpleCache<Dictionary<string, TTokenClass>> _tokenClasses;
        private readonly SimpleCache<PrioTable> _prioTable;
        private readonly SimpleCache<TTokenClass> _listClass;
        private readonly SimpleCache<TTokenClass> _numberClass;
        private readonly DictionaryEx<int, TTokenClass> _leftParenthesis;
        private readonly DictionaryEx<int, TTokenClass> _righParenthesis;

        internal TokenFactory()
        {
            _leftParenthesis = new DictionaryEx<int, TTokenClass>(InternalGetLeftParenthesisClass);
            _righParenthesis = new DictionaryEx<int, TTokenClass>(InternalGetRightParenthesisClass);
            _numberClass = new SimpleCache<TTokenClass>(InternalGetNumberClass);
            _listClass = new SimpleCache<TTokenClass>(InternalGetListClass);
            _prioTable = new SimpleCache<PrioTable>(GetPrioTable);
            _tokenClasses = new SimpleCache<Dictionary<string, TTokenClass>>(InternalGetTokenClasses);
        }

        private Dictionary<string, TTokenClass> InternalGetTokenClasses()
        {
            var result = GetTokenClasses();
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
        ITokenClass ITokenFactory.RightParenthesisClass(int level) { return _righParenthesis.Find(level); }
        ITokenClass ITokenFactory.LeftParenthesisClass(int level) { return _leftParenthesis.Find(level); }

        protected abstract TTokenClass GetNewTokenClass(string name);
        protected abstract PrioTable GetPrioTable();
        protected abstract Dictionary<string, TTokenClass> GetTokenClasses();
        protected abstract TTokenClass GetListClass();
        protected abstract TTokenClass GetRightParenthesisClass(int level);
        protected abstract TTokenClass GetLeftParenthesisClass(int level);
        protected abstract TTokenClass GetNumberClass();

        private Dictionary<string, TTokenClass> TokenClasses { get { return _tokenClasses.Value; } }
        protected ITokenClass TokenClass(string name) { return ((ITokenFactory) this).TokenClass(name); }
    }
}