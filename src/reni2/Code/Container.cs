using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using HWClassLibrary.TreeStructure;
using Reni.Basics;
using Reni.Runtime;

namespace Reni.Code
{
    /// <summary>
    ///     base class for all compiled code items
    /// </summary>
    internal sealed class Container : ReniObject
    {
        private static readonly Container _unexpectedVisitOfPending = new Container("UnexpectedVisitOfPending");
        private readonly string _description;

        /// <summary>
        ///     When set, some exceptions for unserialisable elements are not thrown.
        /// </summary>
        internal readonly bool IsInternal;

        [EnableDump]
        private readonly Size _frameSize;

        [DisableDump]
        private readonly CodeBase _data;

        public Container(CodeBase data, Size frameSize = null, string description = "", bool isInternal = false)
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

        [Node, EnableDump]
        internal CodeBase Data { get { return _data; } }

        [Node, EnableDump]
        internal string Description { get { return _description; } }

        [Node, DisableDump]
        internal bool IsError { get { return _frameSize == null; } }

        [Node, DisableDump]
        public Size MaxSize { get { return _data.MaxSize; } }

        [Node, DisableDump]
        public static Container UnexpectedVisitOfPending { get { return _unexpectedVisitOfPending; } }

        private void DataAdd(FiberItem leafElement) { NotImplementedMethod(leafElement); }

        public CodeTypeDeclaration GetCSharpTypeCode(List<Container> functions, string name, bool useStatementAligner)
        {
            var result = new CodeTypeDeclaration(name);
            result.Comments.Add(new CodeCommentStatement(Generator.NotACommentFlag + "unsafe"));
            result.Members.Add(GetCSharpFunctionCode(Generator.MainFunctionName, false, useStatementAligner));
            for(var i = 0; i < functions.Count; i++)
                result.Members.Add(functions[i].GetCSharpFunctionCode(Generator.FunctionName(i), true, useStatementAligner));
            return result;
        }

        private CodeMemberMethod GetCSharpFunctionCode(string name, bool isFunction, bool useStatementAligner)
        {
            var result = new CodeMemberMethod();
            result.Comments.AddRange(CreateComment(Description));
            result.Comments.Add(new CodeCommentStatement(Generator.NotACommentFlag + "unsafe"));
            result.Name = name;
            result.Attributes = MemberAttributes.Static | MemberAttributes.Public;
            result.Statements.Add(new CodeSnippetExpression(GetCSharpStatements(useStatementAligner, isFunction)));
            if(isFunction)
            {
                result.Parameters.Add(new CodeParameterDeclarationExpression(typeof(DataContainer), Generator.FrameArgName));
                result.Attributes = MemberAttributes.Static | MemberAttributes.Private;
                result.ReturnType = new CodeTypeReference(typeof(DataContainer));
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

            var statements = CSharpGenerator.CreateFunctionBody(_data, isFunction);
            if(useStatementAligner)
                statements = StatementAligner(statements);
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

        internal BitsConst Evaluate()
        {
            NotImplementedMethod();
            return null;
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
    ///     Nothing, since void cannot be used for this purpose
    /// </summary>
    public class none
    {
        private static readonly none _instance = new none();

        /// <summary>
        ///     Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        /// created 03.10.2006 01:24
        public static none Instance { get { return _instance; } }
    }
}