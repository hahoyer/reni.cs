using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
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

        [EnableDumpExcept(null)]
        internal Result<Value> Value
        {
            get
            {
                var declarationItem = Parent.TokenClass as IDeclarationItem;
                if(declarationItem != null && declarationItem.IsDeclarationPart(Parent))
                    return null;

                return (Parent.TokenClass as IValueProvider)?.Get
                    (Parent.Left, Parent.Right, Parent);
            }
        }

        internal Result<Statement[]> GetStatements(List type = null)
            => (Parent.TokenClass as IStatementsProvider)
                ?.Get(type, Parent.Left, Parent.Token.Characters, Parent.Right, Parent);

        [EnableDumpExcept(null)]
        internal Result<Statement[]> Statements => GetStatements();

        [EnableDumpExcept(null)]
        internal Result<Statement> Statement
            => (Parent.TokenClass as IStatementProvider)
                ?.Get(Parent.Left, Parent.Token.Characters, Parent.Right);

        [EnableDumpExcept(null)]
        internal Result<Declarator> Declarator
            => (Parent.TokenClass as IDeclaratorTokenClass)
                ?.Get(Parent.Left, Parent.Token.Characters, Parent.Right);

        [EnableDumpExcept(null)]
        internal Issue[] Issues
            => Value?.Issues
                ?? GetStatements()?.Issues
                    ?? Statement?.Issues
                        ?? Declarator?.Issues
                            ?? new Issue[0];

        [EnableDumpExcept(null)]
        ContextBase[] CompoundContexts
        {
            get
            {
                if(!IsStatementsLevel)
                    return Parent.Parent.Option.CompoundContexts;

                var target = (CompoundSyntax) Parent.Value.Target;

                return target
                    .ResultCache
                    .Values
                    .Select(item => item.Type.ToContext)
                    .ToArray();
            }
        }

        [EnableDumpExcept(null)]
        bool IsStatementsLevel
        {
            get
            {
                if(Value != null)
                    return false;
                if(Statements != null)
                    return Parent.Parent?.TokenClass != Parent.TokenClass;
                if(Statement != null)
                    return false;

                NotImplementedMethod();
                return false;
            }
        }

        [DisableDump]
        internal IEnumerable<string> Declarations
        {
            get
            {
                if(Value != null)
                    return CompoundContexts
                        .SelectMany(item => Value.Target.DeclarationOptions(item));

                if(Statements != null)
                    return CompoundContexts
                        .SelectMany(item => item.DeclarationOptions)
                        .Concat(DeclarationTagToken.DeclarationOptions);

                NotImplementedMethod();
                return null;
            }
        }
    }
}