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
        [DisableDump]
        internal IDefaultScopeProvider DefaultScopeProvider => Owner.DefaultScopeProvider;

        Syntax Owner { get; }

        public SyntaxOption(Syntax owner) { Owner = owner; }

        [EnableDumpExcept(null)]
        internal Result<Value> Value
        {
            get
            {
                var declarationItem = Owner.TokenClass as IDeclarationItem;
                if(declarationItem != null && declarationItem.IsDeclarationPart(Owner))
                    return null;

                return (Owner.TokenClass as IValueProvider)?.Get
                    (Owner);
            }
        }

        internal Result<Statement[]> GetStatements(List type = null)
            => (Owner.TokenClass as IStatementsProvider)?.Get(type, Owner, DefaultScopeProvider);

        [EnableDumpExcept(null)]
        internal Result<Statement[]> Statements => GetStatements();

        [EnableDumpExcept(null)]
        internal Result<Statement> Statement
            =>
            (Owner.TokenClass as IStatementProvider)?.Get
                (Owner.Left, Owner.Right, DefaultScopeProvider);

        [EnableDumpExcept(null)]
        internal Result<Declarator> Declarator
        {
            get
            {
                var declaratorTokenClass = Owner.TokenClass as IDeclaratorTokenClass;
                return declaratorTokenClass?.Get(Owner);
            }
        }

        [EnableDumpExcept(null)]
        internal IDeclarationTag DeclarationTag => Owner.TokenClass as IDeclarationTag;

        [EnableDumpExcept(null)]
        internal Issue[] Issues
            => Value?.Issues
            ?? GetStatements()?.Issues
            ?? Statement?.Issues
            ?? Declarator?.Issues
            ?? new Issue[0];

        [DisableDump]
        ContextBase[] Contexts
        {
            get
            {
                if(IsStatementsLevel)
                    return ((CompoundSyntax) Owner.Value.Target)
                        .ResultCache
                        .Values
                        .Select(item => item.Type.ToContext)
                        .ToArray();

                var parentContexts = Owner.Parent.Option.Contexts;

                if(IsFunctionLevel)
                    return parentContexts
                        .SelectMany(context => FunctionContexts(context, Value.Target))
                        .ToArray();

                return parentContexts;
            }
        }

        static IEnumerable<ContextBase> FunctionContexts(ContextBase context, Value body)
            => ((FunctionBodyType) context.ResultCache(body).Type)
                .Functions
                .Select(item => item.CreateSubContext(false));

        [DisableDump]
        bool IsFunctionLevel => Owner.TokenClass is Function;

        [DisableDump]
        bool IsStatementsLevel
        {
            get
            {
                if(Owner.TokenClass is EndOfText)
                    return true;

                if(Value != null)
                    return false;
                if(Statements != null)
                    return Owner.Parent?.TokenClass != Owner.TokenClass;
                if(Statement != null)
                    return false;

                if(Owner.TokenClass is LeftParenthesis)
                    return false;

                if (Owner.TokenClass is BeginOfText)
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
                    return Contexts
                        .SelectMany(item => Value.Target.DeclarationOptions(item));

                if(Statements != null || Statement != null)
                    return Contexts
                        .SelectMany(item => item.DeclarationOptions)
                        .Concat(DeclarationTagToken.DeclarationOptions);

                if(Declarator != null
                    || Owner.TokenClass is LeftParenthesis
                    || Owner.TokenClass is DeclarationTagToken)
                    return new string[0];

                NotImplementedMethod();
                return null;
            }
        }

        [EnableDump]
        SourcePart Token => Owner.Token.Characters;
    }
}