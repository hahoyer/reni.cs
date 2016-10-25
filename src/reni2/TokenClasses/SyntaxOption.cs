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
        internal IDefaultScopeProvider DefaultScopeProvider => Parent.DefaultScopeProvider;

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
            => (Parent.TokenClass as IStatementsProvider)?.Get(type, Parent, DefaultScopeProvider);

        [EnableDumpExcept(null)]
        internal Result<Statement[]> Statements => GetStatements();

        [EnableDumpExcept(null)]
        internal Result<Statement> Statement
            =>
            (Parent.TokenClass as IStatementProvider)?.Get
                (Parent.Left, Parent.Right, DefaultScopeProvider);

        [EnableDumpExcept(null)]
        internal Result<Declarator> Declarator
        {
            get
            {
                var declaratorTokenClass = Parent.TokenClass as IDeclaratorTokenClass;
                return declaratorTokenClass?.Get(Parent);
            }
        }

        [EnableDumpExcept(null)]
        internal IDeclarationTag DeclarationTag => Parent.TokenClass as IDeclarationTag;

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
                    return ((CompoundSyntax) Parent.Value.Target)
                        .ResultCache
                        .Values
                        .Select(item => item.Type.ToContext)
                        .ToArray();

                if(IsFunctionLevel)
                    return Parent
                        .Parent
                        .Option
                        .Contexts
                        .SelectMany(context => FunctionContexts(context, Value.Target))
                        .ToArray();

                return Parent.Parent.Option.Contexts;
            }
        }

        static IEnumerable<ContextBase> FunctionContexts(ContextBase context, Value body)
            => ((FunctionBodyType) context.ResultCache(body).Type)
                .Functions
                .Select(item => item.CreateSubContext(false));

        [DisableDump]
        bool IsFunctionLevel => Parent.TokenClass is Function;

        [DisableDump]
        bool IsStatementsLevel
        {
            get
            {
                if(Parent.TokenClass is EndOfText)
                    return true;

                if(Value != null)
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
                    return Contexts
                        .SelectMany(item => Value.Target.DeclarationOptions(item));

                if(Statements != null || Statement != null)
                    return Contexts
                        .SelectMany(item => item.DeclarationOptions)
                        .Concat(DeclarationTagToken.DeclarationOptions);

                if(Declarator != null
                    || Parent.TokenClass is LeftParenthesis
                    || Parent.TokenClass is DeclarationTagToken)
                    return new string[0];

                NotImplementedMethod();
                return null;
            }
        }

        [EnableDump]
        SourcePart Token => Parent.Token.Characters;
    }
}