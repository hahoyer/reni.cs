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

        internal FunctionInstance Find(ICompileSyntax body, Structure structure, TypeBase args)
        {
            var index = _data.Find(body).Find(structure, args);
            return _list[index];
        }

        internal List<Code.Container> Compile() { return _list.Select(t => t.Serialize(false)).ToList(); }

        [Serializable]
        private sealed class ContextArgsVariant : ReniObject
        {
            [Node]
            private readonly DictionaryEx<Structure, ArgsVariant> _data;

            public ContextArgsVariant(FunctionList fl, ICompileSyntax body) { _data = new DictionaryEx<Structure, ArgsVariant>(structure => new ArgsVariant(fl, body, structure)); }
            public int Find(Structure structure, TypeBase args) { return _data.Find(structure).Find(args); }
        }

        [Serializable]
        private sealed class ArgsVariant : ReniObject
        {
            [Node]
            private readonly DictionaryEx<TypeBase, int> _data;

            public ArgsVariant(FunctionList fl, ICompileSyntax body, Structure structure)
            {
                _data = new DictionaryEx<TypeBase, int>(-1, args => CreateFunctionInstance(fl, args, body, structure));
            }

            public int Find(TypeBase args) { return _data.Find(args); }

            private static int CreateFunctionInstance(FunctionList fl, TypeBase args, ICompileSyntax body, Structure structure)
            {
                var index = fl._list.Count;
                var f = new FunctionInstance(index, body, structure, args);
                fl._list.Add(f);
                return index;
            }
        }
    }
}