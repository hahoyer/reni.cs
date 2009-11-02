using System;
using System.Collections.Generic;
using System.Linq;
using Reni.Feature;
using Reni.Type;


namespace Reni.Parser.TokenClass
{
    [Token("enable_cut")]
    internal sealed class EnableCut : Defineable, ISearchPath<IFeature, Sequence>
    {
        IFeature ISearchPath<IFeature, Sequence>.Convert(Sequence type) { return type.EnableCutFeature; }
    }

    [Token("<<")]
    internal sealed class ConcatArrays : Defineable, ISearchPath<IFeature, Type.Array>
    {
        IFeature ISearchPath<IFeature, Type.Array>.Convert(Type.Array type) { return new ConcatArraysFeature(type); }
    }

    [Token("<*")]
    internal sealed class ConcatArrayWithObject : Defineable, ISearchPath<IFeature, Type.Array>, ISearchPath<IFeature, Type.Void>
    {
        IFeature ISearchPath<IFeature, Type.Array>.Convert(Type.Array type) { return new ConcatArrayWithObjectFeature(type); }
        IFeature ISearchPath<IFeature, Type.Void>.Convert(Type.Void type) { return new CreateArrayFeature(); }
    }

    [Token(":=")]
    internal sealed class ColonEqual : Defineable, ISearchPath<IFeature, AssignableRef>
    {
        IFeature ISearchPath<IFeature, AssignableRef>.Convert(AssignableRef type) { return type.AssignmentFeature; }
    }
}