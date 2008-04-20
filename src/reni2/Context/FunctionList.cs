using System.Collections.Generic;
using HWClassLibrary.Helper;
using Reni.Code;

namespace Reni.Context
{
    /// <summary>
    /// List of functions
    /// </summary>
    internal sealed class FunctionList
    {
        private readonly DictionaryEx<Syntax.SyntaxBase, EnvArgsVariant> _data =
            new DictionaryEx<Syntax.SyntaxBase, EnvArgsVariant>();

        private readonly List<FunctionInstance> _list = new List<FunctionInstance>();

        /// <summary>
        /// Gets the <see cref="FunctionInstance"/> with the specified i.
        /// </summary>
        /// <value></value>
        /// created 03.01.2007 20:27
        public FunctionInstance this[int i] { get { return _list[i]; } }

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>The count.</value>
        /// created 03.01.2007 20:28
        public int Count { get { return _list.Count; } }

        /// <summary>
        /// Finds the specified body.
        /// </summary>
        /// <param name="body">The body.</param>
        /// <param name="env">The env.</param>
        /// <param name="args">The args.</param>
        /// <returns></returns>
        /// created 03.01.2007 20:26
        internal FunctionInstance Find(Syntax.SyntaxBase body, ContextBase env, Type.TypeBase args)
        {
            EnvArgsVariant eav;
            if(_data.ContainsKey(body))
                eav = _data[body];
            else
            {
                eav = new EnvArgsVariant();
                _data.Add(body, eav);
            }

            return eav.Find(this, env, args, body);
        }

        /// <summary>
        /// Compiles this instance.
        /// </summary>
        /// <returns></returns>
        /// [created 05.06.2006 19:15]
        internal List<Container> Compile()
        {
            var result = new List<Container>();
            for(var i = 0; i < _list.Count; i++)
                result.Add(this[i].Serialize());
            return result;
        }

        #region Nested type: ArgsVariant

        private class ArgsVariant
        {
            private readonly DictionaryEx<Type.TypeBase, int> _data = new DictionaryEx<Type.TypeBase, int>();

            public FunctionInstance Find(FunctionList fl, Type.TypeBase args, Syntax.SyntaxBase body, ContextBase env)
            {
                if(_data.ContainsKey(args))
                    return fl._list[_data[args]];

                var index = fl._list.Count;
                var f = new FunctionInstance(index, body, env, args);
                _data.Add(args, index);
                fl._list.Add(f);

                return f;
            }
        }

        #endregion

        #region Nested type: EnvArgsVariant

        private class EnvArgsVariant : ReniObject
        {
            private readonly DictionaryEx<ContextBase, ArgsVariant> _data = new DictionaryEx<ContextBase, ArgsVariant>();

            public FunctionInstance Find(FunctionList fl, ContextBase env, Type.TypeBase args, Syntax.SyntaxBase body)
            {
                ArgsVariant av;
                if(_data.ContainsKey(env))
                    av = _data[env];
                else
                {
                    av = new ArgsVariant();
                    _data.Add(env, av);
                }

                return av.Find(fl, args, body, env);
            }
        }

        #endregion
    }
}