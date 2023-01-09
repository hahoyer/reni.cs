using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.SyntaxTree;
using Reni.Type;

namespace Reni.Parser;

sealed class RecursionType
    : TypeBase
        , IImplementation
        , IFunction
        , IValue
        , IMeta
        , IContextReference
{
    internal override Root Root { get; }

    IEnumerable<string> InternalDeclarationOptions
    {
        get
        {
            NotImplementedMethod();
            return null;
        }
    }

    public RecursionType(Root root) => Root = root;
    int IContextReference.Order => ObjectId;
    IFunction IEvalImplementation.Function => this;
    IValue IEvalImplementation.Value => this;
    bool IFunction.IsImplicit => false;

    Result IFunction.Result(Category category, TypeBase argsType)
    {
        NotImplementedMethod(category, argsType);
        return null;
    }

    Result IMeta.Result
        (Category category, ResultCache left, ContextBase contextBase, ValueSyntax right)
    {
        NotImplementedMethod(contextBase, left, category, right);
        return null;
    }


    IMeta IMetaImplementation.Function => this;

    Result IValue.Execute(Category category)
    {
        NotImplementedMethod(category);
        return null;
    }

    internal override IEnumerable<string> DeclarationOptions
        => base.DeclarationOptions.Concat(InternalDeclarationOptions);
}