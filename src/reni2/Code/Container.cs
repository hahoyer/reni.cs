using System;
using System.CodeDom;
using System.Collections.Generic;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Type;

namespace Reni.Code
{
    /// <summary>
    /// base class for all compiled code items
    /// </summary>
    [Serializable]
    internal sealed class Container : Visitor<int>
    {
        private static readonly Container _unexpectedVisitOfPending = new Container("UnexpectedVisitOfPending");
        private readonly string _description;

        /// <summary>
        /// When set, some exceptions for unserialisable elements are not thrown. 
        /// </summary>
        internal readonly bool IsInternal;

        [DumpData(true)]
        private readonly Size _frameSize;

        [DumpData(true)]
        private readonly Size _maxSize;

        [DumpData(false)]
        private List<LeafElement> _data = new List<LeafElement>();

        public Container(Size maxSize, Size frameSize, string description, bool isInternal)
        {
            _maxSize = maxSize;
            IsInternal = isInternal;
            _frameSize = frameSize;
            _description = description;
        }

        private Container(string errorText)
        {
            _description = errorText;
            IsInternal = false;
        }

        [Node, DumpData(true)]
        internal List<LeafElement> Data { get { return _data; } }

        [Node, DumpData(true)]
        internal string Description { get { return _description; } }
        [Node, DumpData(false)]
        internal bool IsError { get { return _frameSize == null; } }

        [Node, DumpData(false)]
        public Size MaxSize { get { return _maxSize; } }
        [Node, DumpData(false)]
        public static Container UnexpectedVisitOfPending { get { return _unexpectedVisitOfPending; } }

        private void DataAdd(LeafElement leafElement)
        {
            var toAdd = new List<LeafElement> {leafElement};
            while(toAdd.Count > 0)
            {
                var next = toAdd[0];
                toAdd.RemoveAt(0);

                LeafElement[] newCorrectedElements = null;

                if(_data.Count > 0)
                    newCorrectedElements = _data[_data.Count - 1].TryToCombineN(next);

                if(newCorrectedElements == null)
                    _data.Add(next);
                else
                {
                    _data.RemoveAt(_data.Count - 1);
                    toAdd.InsertRange(0, newCorrectedElements);
                }
            }
        }

        public BitsConst Eval(List<Container> functions)
        {
            NotImplementedMethod(functions);
            throw new NotImplementedException();
        }

        internal override int ContextRef(RefCode visitedObject)
        {
            if(!IsInternal)
                throw new UnexpectedContextRefInContainer(this, visitedObject);
            DataAdd(visitedObject.ToLeafElement);
            return 1;
        }

        internal override int InternalRef(InternalRef visitedObject)
        {
            if(!IsInternal)
                throw new UnexpectedInternalRefInContainer(this, visitedObject);
            DataAdd(visitedObject.ToLeafElement);
            return 1;
        }

        internal override int Child(int parent, LeafElement leafElement) { return parent + Leaf(leafElement); }

        internal override int Leaf(LeafElement leafElement)
        {
            DataAdd(leafElement);
            return 1;
        }

        internal override int Pair(Pair visitedObject, int left, int right) { return left + right; }

        internal override int ThenElse(ThenElse visitedObject, int condResult, int thenResult, int elseResult) { return thenResult; }

        internal override Visitor<int> AfterCond(int objectId)
        {
            DataAdd(new Then(objectId, TypeBase.CreateBit.Size));
            return this;
        }

        internal override Visitor<int> AfterThen(int objectId, Size thenSize)
        {
            DataAdd(new Else(objectId, thenSize));
            return this;
        }

        internal override Visitor<int> AfterElse(int objectId)
        {
            DataAdd(new EndCondional(objectId));
            return this;
        }

        public int Child(Child visitedObject, int parent) { return parent + visitedObject.AddTo(this); }

        public CodeTypeDeclaration GetCSharpTypeCode(List<Container> functions, string name, bool align)
        {
            var result = new CodeTypeDeclaration(name);
            result.Comments.Add(new CodeCommentStatement(Generator.NotACommentFlag + "unsafe"));
            result.Members.Add(GetCSharpFunctionCode(Generator.MainMethodName, false, align));
            for(var i = 0; i < functions.Count; i++)
                result.Members.Add(functions[i].GetCSharpFunctionCode(Generator.FunctionMethodName(i), true, align));
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

            var statements = new StorageDescriptor(MaxSize.ByteAlignedSize, _frameSize).CreateFunctionBody(_data, isFunction);
            if(useStatementAligner)
                statements = StatementAligner(statements);
            statements = statements.Surround("{", "}");
            statements = "fixed(sbyte*data=new sbyte[" + _maxSize.ByteCount + "])" + statements;
            statements = HWString.Indent(statements, 3);
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
            if(_data.Count == 1)
                return _data[0].Evaluate();

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
    internal class ErrorElement : LeafElement
    {
        [Node]
        internal readonly CodeBase CodeBase;
        public ErrorElement(CodeBase codeBase) { CodeBase = codeBase; }
        protected override Size GetSize() { return Size.Zero; }
        protected override Size GetInputSize() { return Size.Zero; }
        protected override bool IsError { get { return true; } }
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