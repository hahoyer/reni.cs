using System;
using System.Collections.Generic;
using System.Linq;
using Bnf.Forms;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using hw.Scanner;

namespace Bnf.Base
{
    sealed class Definitions<T> : DumpableObject
        where T : class, IParseSpan, ISourcePartProxy
    {
        sealed class BnfDefinition : DumpableObject, IDeclaration<T>
        {
            readonly IExpression Expression;
            readonly string Name;

            public BnfDefinition(string name, IExpression expression)
            {
                Name = name;
                Expression = expression;
            }

            string IDeclaration<T>.Name => Name;

            T IDeclaration<T>.Parse(IParserCursor source, IContext<T> context)
                => Expression.Parse(source, context);
        }

        internal readonly IDictionary<string, IDeclaration<T>> Data;

        internal Definitions(IDictionary<string, IExpression> data)
        {
            Data = data
                .ToDictionary(i => i.Key, i => (IDeclaration<T>) new BnfDefinition(i.Key, i.Value));
        }

        public void Register(Type type)
            => Data.AddRange(type.GetBelongings<IDeclaration<T>>().ToDictionary(i => i.Name, i => i));

        public void Register(object target) => Register(target.GetType());
    }
}