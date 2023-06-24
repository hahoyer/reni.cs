using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Reni.Helper;
using Reni.SyntaxTree;

namespace Reni.TokenClasses;

abstract class Semantics : DumpableObject
{
    sealed class Main : Semantics
    {
        readonly CompoundSyntax Parent;

        [EnableDump]
        readonly FunctionCache<string, List<Declaration>> Dictionary = new(_ => new());

        public Main(CompoundSyntax parent)
        {
            Parent = parent;
            Initialize(Parent);
        }


        [DisableDump]
        internal override string Information
        {
            get
            {
                NotImplementedMethod();
                return default;
            }
        }

        internal override Semantics GetSemantics(Syntax syntax)
        {
            if(syntax is DeclarationSyntax declaration)
            {
                if(declaration.Declarer.Name == null)
                {
                    NotImplementedMethod(syntax);
                    return default;
                }

                var declarations = Dictionary[declaration.Declarer.Name.Value];
                if(declarations.Any())
                {
                    NotImplementedMethod(syntax);
                    return default;
                }

                declarations.Add(new()
                    {
                        Position = declaration.MainAnchor.Token.GetDumpAroundCurrent()
                        , IsPublic = declaration.Declarer.IsPublic
                    }
                );
                return GetSemantics(declaration.Value);
            }

            NotImplementedMethod(syntax);
            return default;
            ;
        }
    }

    sealed class Declaration
    {
        public string Position;
        [DisableDumpExcept(true)]
        public bool IsPublic;
    }

    [DisableDump]
    internal abstract string Information { get; }

    internal virtual Semantics GetSemantics(Syntax syntax) => this;

    void Initialize(ITree<Syntax> target)
    {
        if(target == null)
            return;
        foreach(var child in target.GetDirectChildren())
            GetSemantics(child).Initialize(child);
    }

    internal static Semantics From(CompoundSyntax parent) => new Main(parent);
}