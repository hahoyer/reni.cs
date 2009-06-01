using HWClassLibrary.TreeStructure;
using System;
using System.Collections.Generic;
using HWClassLibrary.Helper;
using Reni.Code;
using Reni.Syntax;
using Reni.Type;

namespace Reni.Context
{
    /// <summary>
    /// List of functions
    /// </summary>
    [Serializable]
    internal sealed class FunctionList: ReniObject
    {
        [Node]
        private readonly DictionaryEx<ICompileSyntax, ContextArgsVariant> _data =
            new DictionaryEx<ICompileSyntax, ContextArgsVariant>();

        [Node]
        private readonly List<FunctionInstance> _list = new List<FunctionInstance>();

        internal FunctionInstance this[int i] { get { return _list[i]; } }
        internal int Count { get { return _list.Count; } }

        internal FunctionInstance Find(ICompileSyntax body, ContextBase env, TypeBase args)
        {
            ContextArgsVariant eav;
            if(_data.ContainsKey(body))
                eav = _data[body];
            else
            {
                eav = new ContextArgsVariant();
                _data.Add(body, eav);
            }

            return eav.Find(this, env, args, body);
        }

        internal List<Container> Compile()
        {
            var result = new List<Container>();
            for(var i = 0; i < _list.Count; i++)
                result.Add(this[i].Serialize(false));
            return result;
        }

        [Serializable]
        private class ArgsVariant : ReniObject
        {
            [Node]
            private readonly DictionaryEx<TypeBase, int> _data = new DictionaryEx<TypeBase, int>();

            public FunctionInstance Find(FunctionList fl, TypeBase args, ICompileSyntax body, ContextBase context)
            {
                if(_data.ContainsKey(args))
                    return fl._list[_data[args]];

                var index = fl._list.Count;
                var f = new FunctionInstance(index, body, context, args);
                _data.Add(args, index);
                fl._list.Add(f);

                return f;
            }
        }

        [Serializable]
        private class ContextArgsVariant : ReniObject
        {
            [Node]
            private readonly DictionaryEx<ContextBase, ArgsVariant> _data = new DictionaryEx<ContextBase, ArgsVariant>();

            public FunctionInstance Find(FunctionList fl, ContextBase context, TypeBase args, ICompileSyntax body)
            {
                ArgsVariant av;
                if(_data.ContainsKey(context))
                    av = _data[context];
                else
                {
                    av = new ArgsVariant();
                    _data.Add(context, av);
                }

                return av.Find(fl, args, body, context);
            }
        }
    }
}