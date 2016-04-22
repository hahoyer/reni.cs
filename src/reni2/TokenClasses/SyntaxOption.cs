using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using Reni.Basics;
using Reni.Context;
using Reni.Parser;
using Reni.Struct;
using Reni.Validation;

namespace Reni.TokenClasses
{
    sealed class SyntaxOption : DumpableObject
    {
        Syntax Parent { get; }

        public SyntaxOption(Syntax parent) { Parent = parent; }

        internal Result<Value> Value
        {
            get
            {
                var declarationItem = Parent.TokenClass as IDeclarationItem;
                if(declarationItem != null && declarationItem.IsDeclarationPart(Parent))
                    return null;


                return (Parent.TokenClass as IValueProvider)?.Get
                    (Parent.Left, Parent.Token.Characters, Parent.Right);
            }
        }

        internal Result<Statement[]> GetStatements(List type = null)
            => (Parent.TokenClass as IStatementsProvider)
                ?.Get(type, Parent.Left, Parent.Token.Characters, Parent.Right);

        internal Result<Statement[]> Statements => GetStatements();

        internal Result<Statement> Statement
            => (Parent.TokenClass as IStatementProvider)
                ?.Get(Parent.Left, Parent.Token.Characters, Parent.Right);

        internal Result<Declarator> Declarator
            => (Parent.TokenClass as IDeclaratorTokenClass)
                ?.Get(Parent.Left, Parent.Token.Characters, Parent.Right);

        internal Issue[] Issues
            => Value?.Issues
                ?? GetStatements()?.Issues
                    ?? Statement?.Issues
                        ?? Declarator?.Issues
                            ?? new Issue[0];

        ContextBase[] CompoundContexts
        {
            get
            {
                if(!IsStatementsLevel)
                    return Parent.Parent.Option.CompoundContexts;

                var target = (CompoundSyntax)Parent.Value.Target;

                return target.;
            }
        }

        bool IsStatementsLevel
        {
            get
            {
                if(Value != null)
                    return false;
                if(Statements != null)
                    return Parent.Parent?.TokenClass != Parent.TokenClass;

                NotImplementedMethod();
                return false;
            }
        }


        [DisableDump]
        internal PreType PreType
        {
            get
            {
                if(Value != null)
                {
                    var contexts = CompoundContexts;
                    var types = contexts.Select(item => Value.Target.Type(item)).ToArray();
                    NotImplementedMethod(nameof(types), types);
                    return null;
                }

                NotImplementedMethod();
                return null;
            }
        }

        [DisableDump]
        internal PreContext PreContext
        {
            get
            {
                NotImplementedMethod();
                return null;
            }
        }
    }

    class PreContext {}
}