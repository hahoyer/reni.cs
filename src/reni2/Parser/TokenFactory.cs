using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;

namespace Reni.Parser
{
    internal abstract class TokenFactory<TTokenClass> : ReniObject, ITokenFactory
        where TTokenClass: ITokenClass
    {
        private readonly SimpleCache<Dictionary<string, TTokenClass>> _tokenClasses;
        private readonly SimpleCache<PrioTable> _prioTable;

        internal TokenFactory()
        {
            _prioTable = new SimpleCache<PrioTable>(GetPrioTable);
            _tokenClasses = new SimpleCache<Dictionary<string, TTokenClass>>(GetTokenClasses);
            foreach(var pair in _tokenClasses.Value)
                pair.Value.Name = pair.Key;
        }

        ParserInst ITokenFactory.Parser { get { return new ParserInst(new Scanner(), this); } }
        PrioTable ITokenFactory.PrioTable { get { return _prioTable.Value; } }
        ITokenClass ITokenFactory.TokenClass(string name)
        {
            TTokenClass result;
            if(TokenClasses.TryGetValue(name, out result))
                return result;
            return NewTokenClass(name);
        }
        ITokenClass ITokenFactory.ListClass { get { return GetListClass(); } }
        ITokenClass ITokenFactory.RightParentethesisClass(int level) { return RightParentethesisClass(level);}
        ITokenClass ITokenFactory.LeftParentethesisClass(int level) { return LeftParentethesisClass(level); }

        protected abstract TTokenClass NewTokenClass(string name);
        protected abstract PrioTable GetPrioTable();
        protected abstract Dictionary<string, TTokenClass> GetTokenClasses();
        protected abstract TTokenClass GetListClass();
        protected abstract TTokenClass RightParentethesisClass(int level);
        protected abstract TTokenClass LeftParentethesisClass(int level);

        private Dictionary<string, TTokenClass> TokenClasses { get { return _tokenClasses.Value; } }
    }
}