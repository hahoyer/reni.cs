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
    internal sealed class Array : Defineable, IStructFeature, IConverter<IFeature, Ref>, IFeature
    {
        internal override SearchResult<IStructFeature> SearchFromStruct()
        {
            return SearchResult<IStructFeature>.Success(this, this);
        }

        IConverter<IFeature, Ref> IConverter<IConverter<IFeature, Ref>, Struct.Type>.Convert(Struct.Type type)
        {
            return this;
        }

        IContextFeature IConverter<IContextFeature, StructContextBase>.Convert(StructContextBase type)
        {
            throw new NotImplementedException();
        }

        IFeature IConverter<IFeature, Ref>.Convert(Ref type) { return this; }
        
        Result IFeature.ApplyResult(ContextBase callContext, Category category, ICompileSyntax @object, ICompileSyntax args)
        {
            if(args != null)
                throw new NotImplementedException();
            
            
            return 
        }
    }
}