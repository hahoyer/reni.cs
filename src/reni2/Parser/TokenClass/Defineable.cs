using System;
using HWClassLibrary.Debug;
using Reni.Context;
using Reni.Syntax;
using Reni.Type;

namespace Reni.Parser.TokenClass
{
    /// <summary>
    /// Tokens that can used in definitions (not reserved tokens)
    /// </summary>
    internal abstract class Defineable : Base
    {
        /// <summary>
        /// Gets the name of token for C# generation.
        /// </summary>
        /// <value>The name of the C sharp.</value>
        /// created 08.01.2007 15:02
        [DumpData(false)]
        internal virtual string CSharpNameOfDefaultOperation
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
        [DumpExcept(false)]
        internal string DataFunctionName { get { return GetType().Name; } }

        /// <summary>
        /// Gets the numeric prefix operation.
        /// </summary>
        /// <value>The numeric prefix operation.</value>
        /// created 02.02.2007 23:03
        [DumpExcept(false)]
        internal virtual bool IsBitSequencePrefixOperation { get { return false; } }

        /// <summary>
        /// Gets a value indicating whether this instance is logical operator.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is logical operator; otherwise, <c>false</c>.
        /// </value>
        /// created 03.02.2007 15:22
        [DumpExcept(false)]
        internal virtual bool IsCompareOperator { get { return false; } }

        /// <summary>
        /// Structs the operation.
        /// </summary>
        /// <param name="struc">The struc.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        [DumpExcept(false)]
        internal virtual bool IsStructOperation { get { return false; } }
        ///<summary>
        ///</summary>
        [DumpExcept(false)]
        internal virtual bool IsBitSequenceOperation { get { return false; } }

        [DumpExcept(false)]
        internal virtual bool IsSequenceOperation { get { return false; } }
        /// <summary>
        /// Gets the ref operation.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns></returns>
        /// <value>The ref operation.</value>
        /// created 14.02.2007 02:17
        [DumpExcept(false)]
        internal virtual bool IsRefOperation { get { return false; } }

        /// <summary>
        /// Gets the type operation.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns></returns>
        /// <value>The type operation.</value>
        /// created 07.01.2007 16:24
        [DumpExcept(false)]
        internal virtual bool IsDefaultOperation { get { return false; } }

        /// <summary>
        /// Type.of result of numeric operation, i. e. obj and arg are of type bit array
        /// </summary>
        /// <param name="objSize">Size of the obj.</param>
        /// <param name="argSize">Size of the arg.</param>
        /// <returns></returns>
        /// created 08.01.2007 01:40
        internal virtual Type.Base BitSequenceOperationResultType(int objSize, int argSize)
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

        virtual internal Result VisitDefaultOperationApply(Context.Base callContext, Category category, Syntax.Base args, Type.Base definingType)
        {
            NotImplementedMethod(callContext, category, args, definingType);
            return null;
        }

        virtual internal Result VisitRefOperationApply(Context.Base callContext, Category category, Syntax.Base args, Ref definingType)
        {
            NotImplementedMethod(callContext, category, args, definingType);
            return null;
        }

        virtual internal Result VisitSequenceOperationApply(Context.Base callContext, Category category, Syntax.Base args, Sequence definingType)
        {
            NotImplementedMethod(callContext, category, args, definingType);
            return null;
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

        private Syntax.Base DeclarationSyntax
        {
            get
            {
                if (_declarationSyntax == null)
                    _declarationSyntax = CreateDeclarationSyntax();
                return _declarationSyntax;
            }
        }

        public string Name { get { return _token.Name; } }

        public override Result VirtVisit(Context.Base context, Category category)
        {
            return DeclarationSyntax.Visit(context, category);
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
    }
}