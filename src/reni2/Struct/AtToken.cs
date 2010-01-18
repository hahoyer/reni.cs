using System;
using System.Collections.Generic;
using System.Linq;
using Reni.Feature;
using Reni.Parser.TokenClass;
using Reni.Type;

namespace Reni.Struct
{
    internal sealed class AtToken : Defineable, ISearchPath<IFeature, Type>
    {
        public IFeature Convert(Type type) { return type.AtFeature; }

        internal class Feature : ReniObject, IFeature
        {
            private readonly Type _parent;

            public Feature(Type parent) { _parent = parent; }

            Result IFeature.Apply(Category category)
            {
                NotImplementedMethod(category);
                return null;
            }

            TypeBase IFeature.DefiningType() { return _parent; }
        }
    }
}