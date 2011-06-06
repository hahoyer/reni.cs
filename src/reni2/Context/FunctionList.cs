using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using HWClassLibrary.TreeStructure;
using Reni.Code;
using Reni.Struct;
using Reni.Syntax;
using Reni.Type;

namespace Reni.Context
{
    /// <summary>
    ///     List of functions
    /// </summary>
    [Serializable]
    internal sealed class FunctionList : ReniObject
    {
        [Node]
        private readonly DictionaryEx<ICompileSyntax, ContextArgsVariant> _data;

        [Node]
        private readonly List<FunctionInstance> _list = new List<FunctionInstance>();

        public FunctionList() { _data = new DictionaryEx<ICompileSyntax, ContextArgsVariant>(body => new ContextArgsVariant(this, body)); }

        internal FunctionInstance this[int i] { get { return _list[i]; } }
        internal int Count { get { return _list.Count; } }
        internal CodeBase[] Code { get { return _list.Select(t => t.BodyCode).ToArray(); } }

        internal FunctionInstance Find(ICompileSyntax body, Struct.Context context, TypeBase args)
        {
            var index = _data.Find(body).Find(context, args);
            return _list[index];
        }

        internal List<Code.Container> Compile() { return _list.Select(t => t.Serialize(false)).ToList(); }

        [Serializable]
        private sealed class ContextArgsVariant : ReniObject
        {
            [Node]
            private readonly DictionaryEx<Struct.Context, ArgsVariant> _data;

            public ContextArgsVariant(FunctionList fl, ICompileSyntax body) { _data = new DictionaryEx<Struct.Context, ArgsVariant>(context => new ArgsVariant(fl, body, context)); }
            public int Find(Struct.Context context, TypeBase args) { return _data.Find(context).Find(args); }
        }

        [Serializable]
        private class ArgsVariant : ReniObject
        {
            [Node]
            private readonly DictionaryEx<TypeBase, int> _data;

            public ArgsVariant(FunctionList fl, ICompileSyntax body, Struct.Context context) { _data = new DictionaryEx<TypeBase, int>(-1, args => CreateFunctionInstance(fl, args, body, context)); }

            public int Find(TypeBase args) { return _data.Find(args); }

            private static int CreateFunctionInstance(FunctionList fl, TypeBase args, ICompileSyntax body, Struct.Context context)
            {
                var index = fl._list.Count;
                var f = new FunctionInstance(index, body, context, args);
                fl._list.Add(f);
                return index;
            }
        }
    }
}