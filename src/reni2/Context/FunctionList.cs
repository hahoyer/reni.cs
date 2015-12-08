using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Forms;
using hw.Helper;
using Reni.Code;
using Reni.Struct;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Context
{
    sealed class FunctionList : DumpableObject
    {
        [Node]
        readonly FunctionCache<FunctionSyntax, FunctionCache<CompoundView, FunctionCache<TypeBase, int>>> _dictionary;

        [Node]
        readonly List<FunctionType> _list = new List<FunctionType>();

        public FunctionList()
        {
            _dictionary = new FunctionCache<FunctionSyntax, FunctionCache<CompoundView, FunctionCache<TypeBase, int>>>
                (
                syntax => new FunctionCache<CompoundView, FunctionCache<TypeBase, int>>
                    (
                    structure => new FunctionCache<TypeBase, int>
                        (-1, args => CreateFunctionInstance(args, syntax, structure))));
        }

        internal int Count => _list.Count;

        internal FunctionType Find(FunctionSyntax syntax, CompoundView compoundView, TypeBase argsType)
        {
            var index = _dictionary[syntax][compoundView][argsType];
            return _list[index];
        }

        internal FunctionContainer Container(int index) => _list[index].Container;
        internal FunctionType Item(int index) => _list[index];

        int CreateFunctionInstance(TypeBase args, FunctionSyntax syntax, CompoundView compoundView)
        {
            var index = _list.Count;
            var f = new FunctionType(index, syntax, compoundView, args);
            _list.Add(f);
            return index;
        }
    }
}