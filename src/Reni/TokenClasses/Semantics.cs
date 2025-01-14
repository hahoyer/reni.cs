using Reni.Context;
using Reni.Helper;
using Reni.SyntaxTree;

namespace Reni.TokenClasses;

sealed class Semantics : DumpableObject
{
    sealed class Declaration
    {
        public readonly List<Usage> Usages = [];
        public string? Position;

        [DisableDumpExcept(true)]
        public bool IsPublic;

        [DisableDump]
        public BinaryTree? Anchor;
    }

    sealed class Usage
    {
        public string? Position;

        [DisableDump]
        public BinaryTree? Anchor;
    }

    [EnableDumpExcept(null)]
    internal string? Information;

    [EnableDump]
    readonly FunctionCache<string, List<Declaration>> Dictionary = new(_ => []);

    Semantics(ContextBase context, ValueSyntax? parent) => Initialize(context, parent);

    void Initialize(ContextBase context, Syntax? target)
    {
        if(target == null)
            return;

        foreach(var child in target.GetDirectChildren())
            Initialize(context, child);

        FlatInitialize(context, target);
    }

    void FlatInitialize(ContextBase context, Syntax? target)
    {
        switch(target)
        {
            case DeclarationSyntax declaration when declaration.Declarer?.Name == null:
                NotImplementedMethod(target);
                return;
            case DeclarationSyntax declaration:
                FlatInitialize(declaration);
                return;
            case InfixSyntax:
            case SuffixSyntax:
                FlatInitialize(context, (ValueSyntax)target);
                return;
            case ExpressionSyntax expression:
                FlatInitialize(context, expression);
                return;
            case TerminalSyntax:
            case CompoundSyntax:
            case DeclarerSyntax.NameSyntax:
            case DeclarerSyntax.TagSyntax:
                return;
            default:
                NotImplementedMethod(target);
                return;
        }
    }

    void FlatInitialize(DeclarationSyntax target)
    {
        var declarations = Dictionary[target.Declarer!.Name!.Value];
        if(declarations.Any())
        {
            NotImplementedMethod(target);
            return;
        }

        declarations.Add(new()
            {
                Position = target.MainAnchor.Token.GetDumpAroundCurrent()
                , Anchor = target.MainAnchor
                , IsPublic = target.Declarer.IsPublic
            }
        );
        return;
    }

    void FlatInitialize(ContextBase context, ValueSyntax target)
    {
        var declarations = Dictionary[target.MainAnchor.Token.Id];
        if(!declarations.Any())
            declarations.Add(new());
        var declaration = declarations.Single();
        declaration.Usages.Add(new()
        {
            Position = target.MainAnchor.Token.GetDumpAroundCurrent()
            , Anchor = target.MainAnchor
        });


        return;
    }

    void FlatInitialize(ContextBase context, ExpressionSyntax target)
    {
        if(target.Left == null)
        {
            NotImplementedMethod(context, target);
            return;
        }

        var type = target.Left.GetTypeBase(context);

        NotImplementedMethod(context, target, "type", type);
        return;
    }

    internal static Semantics From(ContextBase context, ValueSyntax? parent) => new(context, parent);
}