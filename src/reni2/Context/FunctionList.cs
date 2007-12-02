using System.Collections.Generic;
using Reni.Code;

namespace Reni.Context
{
    /// <summary>
    /// List of functions
    /// </summary>
    internal sealed class FunctionList
    {
        List<FunctionInstance> _list = new List<FunctionInstance>();
        HWClassLibrary.Helper.DictionaryEx<Syntax.Base, EnvArgsVariant> _data = new HWClassLibrary.Helper.DictionaryEx<Syntax.Base, EnvArgsVariant>();

        /// <summary>
        /// Finds the specified body.
        /// </summary>
        /// <param name="body">The body.</param>
        /// <param name="env">The env.</param>
        /// <param name="args">The args.</param>
        /// <returns></returns>
        /// created 03.01.2007 20:26
        internal FunctionInstance Find(Syntax.Base body, Base env, Type.Base args)
        {
            EnvArgsVariant eav;
            if (_data.ContainsKey(body))
                eav = _data[body];
            else
            {
                eav = new EnvArgsVariant();
                _data.Add(body, eav);
            }

            return eav.Find(this, env, args, body);
        }

        private class EnvArgsVariant: ReniObject
        {
            HWClassLibrary.Helper.DictionaryEx<Base, ArgsVariant> _data = new HWClassLibrary.Helper.DictionaryEx<Base, ArgsVariant>();

            public FunctionInstance Find(FunctionList fl, Base env, Type.Base args, Syntax.Base body)
            {
                ArgsVariant av;
                if(_data.ContainsKey(env))
                    av = _data[env];
                else
                {
                    av = new ArgsVariant();
                    _data.Add(env,av);
                }

                return av.Find(fl,args, body, env);
            }

        }
    
        private class ArgsVariant
        {
            HWClassLibrary.Helper.DictionaryEx<Type.Base, int> _data = new HWClassLibrary.Helper.DictionaryEx<Type.Base, int>();

            public FunctionInstance Find(FunctionList fl, Type.Base args, Syntax.Base body, Base env)
            {
                if(_data.ContainsKey(args))
                    return fl._list[_data[args]];

                int index = fl._list.Count;
                FunctionInstance f = new FunctionInstance(index, body, env, args);
                _data.Add(args,index);
                fl._list.Add(f);
                
                return f;
            }
        }

        /// <summary>
        /// Gets the <see cref="FunctionInstance"/> with the specified i.
        /// </summary>
        /// <value></value>
        /// created 03.01.2007 20:27
        public FunctionInstance this[int i]{get{return _list[i];}}

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>The count.</value>
        /// created 03.01.2007 20:28
        public int Count { get { return _list.Count; } }
        /// <summary>
        /// Compiles this instance.
        /// </summary>
        /// <returns></returns>
        /// [created 05.06.2006 19:15]
        internal List<Container> Compile()
        {
            List<Container> result = new List<Container>();
            for (int i = 0; i < _list.Count; i++)
                result.Add(this[i].Serialize());
            return result;
        }
    }
}