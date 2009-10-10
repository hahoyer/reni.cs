using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Helper;
using HWClassLibrary.TreeStructure;
using Reni.Code;
using Reni.Syntax;
using Reni.Type;

namespace Reni.Context
{
    /// <summary>
    /// List of functions
    /// </summary>
    [Serializable]
    internal sealed class FunctionList : ReniObject
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
            var index = _data.Find(body, () => new ContextArgsVariant()).Find(this, env, args, body);
            return _list[index];
        }

        internal List<Container> Compile()
        {
            var result = new List<Container>();
            for(var i = 0; i < _list.Count; i++)
                result.Add(this[i].Serialize(false));
            return result;
        }

        [Serializable]
        private class ContextArgsVariant : ReniObject
        {
            [Node]
            private readonly DictionaryEx<ContextBase, ArgsVariant> _data = new DictionaryEx<ContextBase, ArgsVariant>();

            public int Find(FunctionList fl, ContextBase context, TypeBase args, ICompileSyntax body) { return _data.Find(context, () => new ArgsVariant()).Find(fl, args, body, context); }
        }

        [Serializable]
        private class ArgsVariant : ReniObject
        {
            [Node]
            private readonly DictionaryEx<TypeBase, int> _data = new DictionaryEx<TypeBase, int>(-1);

            public int Find(FunctionList fl, TypeBase args, ICompileSyntax body, ContextBase context)
            {
                return _data.Find(args, () => CreateFunctionInstance(fl, args, body, context));
            }

            private static int CreateFunctionInstance(FunctionList fl, TypeBase args, ICompileSyntax body, ContextBase context)
            {
                var index = fl._list.Count;
                var f = new FunctionInstance(index, body, context, args);
                fl._list.Add(f);
                return index;
            }
        }
    }
}