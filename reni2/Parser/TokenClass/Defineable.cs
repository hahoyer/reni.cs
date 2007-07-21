﻿using System;
using HWClassLibrary.Debug;
using Reni.Context;
using Reni.Parser;
using Reni.Syntax;
using Reni.Type;
using Void=Reni.Syntax.Void;

namespace Reni.Parser.TokenClass
{
    /// <summary>
    /// Tokens that can used in definitions (not reserved tokens)
    /// </summary>
    public abstract class Defineable: Base
    {
        /// <summary>
        /// Structs the operation.
        /// </summary>
        /// <param name="struc">The struc.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        internal virtual StructSearchResult StructOperation(Context.Struct struc)
        {
            return null;
        }

        ///<summary>
        ///</summary>
        public virtual SearchResult NumericOperation(Type.Base obj)
        {
            return null;
        }

        /// <summary>
        /// Gets the ref operation.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns></returns>
        /// <value>The ref operation.</value>
        /// created 14.02.2007 02:17
        public virtual SearchResult RefOperation(Ref obj)
        {
            return null;
        }

        /// <summary>
        /// Gets the name of token for C# generation.
        /// </summary>
        /// <value>The name of the C sharp.</value>
        /// created 08.01.2007 15:02
        [DumpData(false)]
        virtual public string CSharpNameOfDefaultOperation
        {
            get
            {
                NotImplementedMethod();
                return "";
            }
        }

        /// <summary>
        /// Gets the name of function defined in class data for C# code generation.for odd sized numbers
        /// </summary>
        /// <value>The name of the C sharp.</value>
        /// created 08.01.2007 15:02
        [DumpData(false)]
        public string DataFunctionName
        {
            get
            {
                return GetType().Name;
            }
        }

        /// <summary>
        /// Gets the type operation.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns></returns>
        /// <value>The type operation.</value>
        /// created 07.01.2007 16:24
        virtual public SearchResult DefaultOperation(Type.Base obj)
        {
            return null;
        }

        /// <summary>
        /// Gets the numeric prefix operation.
        /// </summary>
        /// <value>The numeric prefix operation.</value>
        /// created 02.02.2007 23:03
        [DumpData(false)]
        internal virtual PrefixSearchResult NumericPrefixOperation { get { return null; } }

        /// <summary>
        /// Gets a value indicating whether this instance is logical operator.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is logical operator; otherwise, <c>false</c>.
        /// </value>
        /// created 03.02.2007 15:22
        [DumpData(false)]
        virtual public bool IsCompareOperator { get { return false; } }
    
        /// <summary>
        /// Type.of result of numeric operation, i. e. obj and arg are of type bit array
        /// </summary>
        /// <param name="objSize">Size of the obj.</param>
        /// <param name="argSize">Size of the arg.</param>
        /// <returns></returns>
        /// created 08.01.2007 01:40
        virtual public Type.Base NumericOperationResultType(int objSize, int argSize)
        {
            NotImplementedMethod(objSize, argSize);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates the syntax.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="token">The token.</param>
        /// <param name="right">The right.</param>
        /// <returns></returns>
        /// created 31.03.2007 14:02 on SAPHIRE by HH
        public override Syntax.Base CreateSyntax(Syntax.Base left, Token token, Syntax.Base right)
        {
            if (left != null)
                return left.CreateDefinableSyntax(new DefineableToken(token), right);
            if (right != null)
                return new Statement(new MemberElem(new DefineableToken(token), right));
            return new DefinableTokenSyntax(token);
        }
    }

    internal class DefinableTokenSyntax : Syntax.Base
    {
        private readonly Token _token;
        private Syntax.Base _declarationSyntax;

        public DefinableTokenSyntax(Token token)
        {
            _token = token;
        }

        public override Result VirtVisit(Context.Base context, Category category)
        {
            return DeclarationSyntax.Visit(context, category);
        }

        private Syntax.Base DeclarationSyntax
        {
            get
            {
                if (_declarationSyntax == null)
                    _declarationSyntax = CreateDeclarationSyntax();
                return _declarationSyntax;
            }
        }

        internal override Syntax.Base CreateDefinableSyntax(DefineableToken defineableToken, Syntax.Base right)
        {
            return new Statement(CreateMemberElem(), new MemberElem(defineableToken, right));
        }

        public override Syntax.Base CreateDeclarationSyntax(Token token, Syntax.Base right)
        {
            return new DeclarationSyntax(CreateDefineableToken(), token, right);
        }

        private Syntax.Base CreateDeclarationSyntax()
        {
            return new Statement(CreateMemberElem());
        }

        private MemberElem CreateMemberElem()
        {
            return new MemberElem(CreateDefineableToken(), null);
        }

        private DefineableToken CreateDefineableToken()
        {
            return new DefineableToken(_token);
        }

        /// <summary>
        /// Dumps the short.
        /// </summary>
        /// <returns></returns>
        /// created 07.05.2007 22:09 on HAHOYER-DELL by hh
        internal override string DumpShort()
        {
            return _token.Name;
        }

        public string Name { get { return _token.Name; } }
    }

    internal class DeclarationSyntax : Syntax.Base
    {
        private readonly DefineableToken _defineableToken;
        private readonly Token _token;
        private readonly Syntax.Base _definition;

        public DeclarationSyntax(DefineableToken defineableToken, Token token, Syntax.Base definition)
        {
            _defineableToken = defineableToken;
            _token = token;
            _definition = definition;
            StopByObjectId(-876);
        }

        public DefineableToken DefineableToken { get { return _defineableToken; } }
        public Token Token { get { return _token; } }
        public Syntax.Base Definition { get { return _definition; } }

        public override Result VirtVisit(Context.Base context, Category category)
        {
            NotImplementedMethod(context, category);
            return null;
        }

        internal override Syntax.Base CreateListSyntax(Token token, Syntax.Base right)
        {
            if (right == null)
                return Reni.Syntax.Struct.Create(this,new Void(token));
            return right.CreateListSyntaxReverse(this, token);
        }

        internal override Syntax.Base CreateListSyntaxReverse(Syntax.Base left, Token token)
        {
            return CreateListSyntax(this).CreateListSyntaxReverse(left, token);
        }

        internal override Syntax.Base CreateListSyntaxReverse(DeclarationSyntax left, Token token)
        {
            return CreateListSyntax(this).CreateListSyntaxReverse(left, token);
        }

        /// <summary>
        /// Dumps the short.
        /// </summary>
        /// <returns></returns>
        /// created 07.05.2007 22:09 on HAHOYER-DELL by hh
        internal override string DumpShort()
        {
            return DefineableToken.Name + ": "+ Definition.DumpShort();
        }

        /// <summary>
        /// What to when syntax element is surrounded by parenthesis?
        /// </summary>
        /// <returns></returns>
        /// created 19.07.2007 23:20 on HAHOYER-DELL by hh
        public override Syntax.Base SurroundedByParenthesis()
        {
            return CreateListSyntax(this);
        }

    }
}

