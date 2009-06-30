using HWClassLibrary.Debug;
using System;
using Reni.Context;
using Reni.Feature;
using Reni.Struct;
using Reni.Syntax;
using Reni.Type;

namespace Reni.Parser.TokenClass.Name
{
    [Token("array")]
    internal sealed class Array : Defineable, IStructFeature
    {
        internal override SearchResult<IStructFeature> SearchFromStruct()
        {
            return SearchResult<IStructFeature>.Success(this, this);
        }

        IConverter<IFeature, Ref> IConverter<IConverter<IFeature, Ref>, Struct.Type>.Convert(Struct.Type type)
        {
            return type.ArrayFeature;
        }
        IContextFeature IConverter<IContextFeature, StructContextBase>.Convert(StructContextBase type) { throw new NotImplementedException(); }
    }
}