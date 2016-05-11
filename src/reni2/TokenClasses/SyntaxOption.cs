using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;
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
                    (Parent);
            }
        }

        internal Result<Statement[]> GetStatements(List type = null)
            => (Parent.TokenClass as IStatementsProvider)?.Get(type, Parent);

        [EnableDumpExcept(null)]
        internal Result<Statement[]> Statements => GetStatements();

        [EnableDumpExcept(null)]
        internal Result<Statement> Statement
            => (Parent.TokenClass as IStatementProvider)?.Get(Parent.Left, Parent.Right);

        [EnableDumpExcept(null)]
        internal Result<Declarator> Declarator
            => (Parent.TokenClass as IDeclaratorTokenClass)?.Get(Parent);

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
                if ((Parent.TokenClass as RightParenthesis)?.IsFrameToken ?? false)
                    return true;

                if (Value != null)
                    return false;
                if(Statements != null)
                    return Parent.Parent?.TokenClass != Parent.TokenClass;
                if(Statement != null)
                    return false;

                var leftParenthesis = Parent.TokenClass as LeftParenthesis;
                if(leftParenthesis != null)
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

                if(Statements != null || Statement != null)
                    return CompoundContexts
                        .SelectMany(item => item.DeclarationOptions)
                        .Concat(DeclarationTagToken.DeclarationOptions);
                if(Declarator != null || Parent.TokenClass is LeftParenthesis)
                    return new string[0];

                NotImplementedMethod();
                return null;
            }
        }

        [EnableDump]
        SourcePart Token => Parent.Token.Characters;
    }
}