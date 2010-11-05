using System;
using System.CodeDom;
using System.Collections.Generic;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using HWClassLibrary.TreeStructure;
using Reni.Type;

namespace Reni.Code
{
    /// <summary>
    /// base class for all compiled code items
    /// </summary>
    internal sealed class Container : ReniObject
    {
        private static readonly Container _unexpectedVisitOfPending = new Container("UnexpectedVisitOfPending");
        private readonly string _description;

        /// <summary>
        /// When set, some exceptions for unserialisable elements are not thrown. 
        /// </summary>
        internal readonly bool IsInternal;

        [IsDumpEnabled(true)]
        private readonly Size _frameSize;

        [IsDumpEnabled(false)]
        private readonly CodeBase _data;

        public Container(CodeBase data, Size frameSize = null, string description = "", bool isInternal= false)
        {
            IsInternal = isInternal;
            _frameSize = frameSize ?? Size.Zero;
            _description = description;
            _data = data;
        }

        private Container(string errorText)
        {
            _description = errorText;
            IsInternal = false;
        }

        [Node, IsDumpEnabled(true)]
        internal CodeBase Data { get { return _data; } }

        [Node, IsDumpEnabled(true)]
        internal string Description { get { return _description; } }
        [Node, IsDumpEnabled(false)]
        internal bool IsError { get { return _frameSize == null; } }

        [Node, IsDumpEnabled(false)]
        public Size MaxSize { get { return _data.MaxSize; } }
        [Node, IsDumpEnabled(false)]
        public static Container UnexpectedVisitOfPending { get { return _unexpectedVisitOfPending; } }

        private void DataAdd(FiberItem leafElement)
        {
            NotImplementedMethod(leafElement);
        }

        public CodeTypeDeclaration GetCSharpTypeCode(List<Container> functions, string name, bool useStatementAligner)
        {
            var result = new CodeTypeDeclaration(name);
            result.Comments.Add(new CodeCommentStatement(Generator.NotACommentFlag + "unsafe"));
            result.Members.Add(GetCSharpFunctionCode(Generator.MainFunctionName, false, useStatementAligner));
            for(var i = 0; i < functions.Count; i++)
                result.Members.Add(functions[i].GetCSharpFunctionCode(Generator.FunctionName(i), true, useStatementAligner));
            return result;
        }

        public CodeMemberMethod GetCSharpFunctionCode(string name, bool isFunction, bool useStatementAligner)
        {
            var result = new CodeMemberMethod();
            result.Comments.AddRange(CreateComment(Description));
            result.Name = name;
            result.Attributes = MemberAttributes.Static | MemberAttributes.Public;
            result.Statements.Add(new CodeSnippetExpression(GetCSharpStatements(useStatementAligner, isFunction)));
            if(isFunction)
            {
                result.Parameters.Add(new CodeParameterDeclarationExpression(typeof(sbyte*), "frame"));
                result.Attributes = MemberAttributes.Static | MemberAttributes.Private;
            }
            return result;
        }

        private static CodeCommentStatementCollection CreateComment(string description)
        {
            var lines = description.Split('\n');
            var result = new CodeCommentStatementCollection();
            for(var i = 0; i < lines.Length; i++)
                result.Add(new CodeCommentStatement(lines[i]));
            return result;
        }

        private string GetCSharpStatements(bool useStatementAligner, bool isFunction)
        {
            if(IsError)
                return "throw new Exception(" + Description.Replace("\"", "\"\"") + ")";

            var statements = new CSharpGenerator(MaxSize.ByteAlignedSize, _frameSize).CreateFunctionBody(_data, isFunction);
            if(useStatementAligner)
                statements = StatementAligner(statements);
            statements = statements.Surround("{", "}");
            statements = "fixed(sbyte*data=new sbyte[" + MaxSize.ByteCount + "])" + statements;
            statements = statements.Indent(3);
            return statements;
        }

        private static string StatementAligner(string statements)
        {
            var aligner = new StringAligner();
            aligner.AddFloatingColumn("frame", "data");
            aligner.AddFloatingColumn(" ", ")");
            aligner.AddFloatingColumn("; // ");
            aligner.AddFloatingColumn(" ");
            return aligner.Format(statements);
        }

        internal sealed class UnexpectedContextRefInContainer : CodeBaseException
        {
            internal UnexpectedContextRefInContainer(Container container, CodeBase visitedObject)
                : base(container, visitedObject) { }
        }

        internal BitsConst Evaluate()
        {
            NotImplementedMethod();
            return null;
        }

        internal abstract class CodeBaseException : Exception
        {
            private readonly Container _container;
            private readonly CodeBase _visitedObject;

            protected CodeBaseException(Container container,
                CodeBase visitedObject)
            {
                _container = container;
                _visitedObject = visitedObject;
            }

            internal Container Container { get { return _container; } }
            internal CodeBase VisitedObject { get { return _visitedObject; } }
        }

        internal sealed class UnexpectedInternalRefInContainer : CodeBaseException
        {
            public UnexpectedInternalRefInContainer(Container container, CodeBase visitedObject)
                : base(container, visitedObject) { }
        }

    }

    [Serializable]
    internal class ErrorElement : FiberHead
    {
        [Node]
        internal readonly CodeBase CodeBase;
        public ErrorElement(CodeBase codeBase) { CodeBase = codeBase; }
        protected override Size GetSize() { return Size.Zero; }
    }

    /// <summary>
    /// Nothing, since void cannot be used for this purpose
    /// </summary>
    public class none
    {
        private static readonly none _instance = new none();

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        /// created 03.10.2006 01:24
        public static none Instance { get { return _instance; } }
    }
}