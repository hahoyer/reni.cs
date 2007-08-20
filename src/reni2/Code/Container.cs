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
    internal sealed class Container : Visitor<int>
    {
        [DumpData(true)]
        private List<LeafElement> _data = new List<LeafElement>();

        /// <summary>
        /// Initializes a new instance of the Container class.
        /// </summary>
        /// <param name="maxSize">Size of the max.</param>
        /// <param name="frameSize">Size of the args.</param>
        /// <param name="description">The description.</param>
        /// created 26.11.2006 17:05
        public Container(Size maxSize, Size frameSize, string description)
        {
            _maxSize = maxSize;
            _frameSize = frameSize;
            _description = description;
        }

        [DumpData(true)]
        private readonly Size _maxSize;
        [DumpData(true)]
        private readonly Size _frameSize;

        private readonly string _description;
        private static Container _unexpectedVisitOfPending = new Container("UnexpectedVisitOfPending");

        private Container(string errorText)
        {
            _description = errorText;
        }

        /// <summary>
        /// Gets the data.
        /// </summary>
        /// <param name="leafElement">The leaf element.</param>
        /// <value>The data.</value>
        /// created 03.10.2006 21:09
        void DataAdd(LeafElement leafElement)
        {
            int count = _data.Count;
            while(count > 0)
            {
                count--;
                LeafElement newElement = _data[count].TryToCombine(leafElement);
                if (newElement == null)
                    count = 0;
                else
                {
                    leafElement = newElement;
                    _data.RemoveAt(count);
                }
            }
            _data.Add(leafElement);
        }

        /// <summary>
        /// Evals the container by use specified functions.
        /// </summary>
        /// <param name="functions">The functions.</param>
        /// <returns></returns>
        /// created 03.10.2006 00:43
        public BitsConst Eval(List<Container> functions)
        {
            NotImplementedMethod(functions);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Contexts the ref.
        /// </summary>
        /// <param name="visitedObject">The visited object.</param>
        /// <returns></returns>
        /// created 17.10.2006 00:04
        internal override int ContextRef<C>(ContextRef<C> visitedObject)
        {
            throw new UnexpectedContextRefInContainer(this, visitedObject);
        }

        internal class UnexpectedContextRefInContainer : Exception
        {
            private readonly Container _container;
            private readonly Base _visitedObject;

            internal UnexpectedContextRefInContainer(Container container, Base visitedObject)
            {
                _container = container;
                _visitedObject = visitedObject;
            }

            internal Container Container { get { return _container; } }
            internal Base VisitedObject { get { return _visitedObject; } }
        }

        /// <summary>
        /// Childs the specified parent.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="leafElement">The leaf element.</param>
        /// <returns></returns>
        /// created 06.10.2006 00:18
        internal override int Child(int parent, LeafElement leafElement)
        {
            return parent + Leaf(leafElement);
        }

        /// <summary>
        /// Leafs the specified leaf element.
        /// </summary>
        /// <param name="leafElement">The leaf element.</param>
        /// <returns></returns>
        /// created 06.10.2006 00:22
        internal override int Leaf(LeafElement leafElement)
        {
            DataAdd(leafElement);
            return 1;
        }

        /// <summary>
        /// Sequences the specified visited object.
        /// </summary>
        /// <param name="visitedObject">The visited object.</param>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns></returns>
        /// created 03.10.2006 01:39
        internal override int Pair(Pair visitedObject, int left, int right)
        {
            return left + right;
        }

        /// <summary>
        /// Thens the else.
        /// </summary>
        /// <param name="visitedObject">The visited object.</param>
        /// <param name="condResult">The cond result.</param>
        /// <param name="thenResult">The then result.</param>
        /// <param name="elseResult">The else result.</param>
        /// <returns></returns>
        /// created 09.01.2007 04:54
        internal override int ThenElse(ThenElse visitedObject, int condResult, int thenResult, int elseResult)
        {
            return thenResult;
        }

        /// <summary>
        /// Afters the cond.
        /// </summary>
        /// <param name="objectId">The object id.</param>
        /// <returns></returns>
        /// created 09.01.2007 04:52
        internal override Visitor<int> AfterCond(int objectId)
        {
            DataAdd(new Then(objectId,Bit.CreateBit.Size));
            return this;
        }

        /// <summary>
        /// Afters the cond.
        /// </summary>
        /// <param name="objectId">The object id.</param>
        /// <param name="thenSize">Size of the then.</param>
        /// <returns></returns>
        /// created 09.01.2007 04:52
        internal override Visitor<int> AfterThen(int objectId, Size thenSize)
        {
            DataAdd(new Else(objectId,thenSize));
            return this;
        }

        /// <summary>
        /// Afters the cond.
        /// </summary>
        /// <param name="objectId">The object id.</param>
        /// <returns></returns>
        /// created 09.01.2007 04:52
        internal override Visitor<int> AfterElse(int objectId)
        {
            DataAdd(new EndCondional(objectId));
            return this;
        }
        /// <summary>
        /// Childs the specified visited object.
        /// </summary>
        /// <param name="visitedObject">The visited object.</param>
        /// <param name="parent">The parent.</param>
        /// <returns></returns>
        /// created 03.10.2006 02:50
        public int Child(Child visitedObject, int parent)
        {
            return parent + visitedObject.AddTo(this);
        }


        /// <summary>
        /// Formats the specified functions.
        /// </summary>
        /// <param name="functions">The functions.</param>
        /// <param name="name">The name.</param>
        /// <param name="align">if set to <c>true</c> [align].</param>
        /// <returns></returns>
        /// created 07.10.2006 20:54
        public CodeTypeDeclaration GetCSharpTypeCode(List<Container> functions, string name, bool align)
        {
            CodeTypeDeclaration result = new CodeTypeDeclaration(name);
            result.Comments.Add(new CodeCommentStatement(Generator.NotACommentFlag+"unsafe"));
            result.Members.Add(GetCSharpFunctionCode(Generator.MainMethodName, false, align));
            for (int i = 0; i < functions.Count; i++)
                result.Members.Add(functions[i].GetCSharpFunctionCode(Generator.FunctionMethodName(i), true, align));
            return result;
        }

        /// <summary>
        /// Formats this instance.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="isFunction">if set to <c>true</c> [is function].</param>
        /// <param name="useStatementAligner">if set to <c>true</c> [useStatementAligner].</param>
        /// <returns></returns>
        /// created 07.10.2006 21:10
        public CodeMemberMethod GetCSharpFunctionCode(string name, bool isFunction, bool useStatementAligner)
        {
            CodeMemberMethod result = new CodeMemberMethod();
            result.Comments.AddRange(CreateComment(Description));
            result.Name = name;
            result.Attributes = MemberAttributes.Static | MemberAttributes.Public;
            result.Statements.Add(new CodeSnippetExpression(GetCSharpStatements(useStatementAligner,isFunction)));
            if (isFunction)
            {
                result.Parameters.Add(new CodeParameterDeclarationExpression(typeof (sbyte*), "frame"));
                result.Attributes = MemberAttributes.Static | MemberAttributes.Private;
            }
            return result;
        }

        private string Description { get { return _description; } }

        private static CodeCommentStatementCollection CreateComment(string description)
        {
            string[] lines = description.Split('\n');
            CodeCommentStatementCollection result = new CodeCommentStatementCollection();
            for (int i = 0; i < lines.Length; i++)
                result.Add(new CodeCommentStatement(lines[i]));
            return result;
        }

        private string GetCSharpStatements(bool useStatementAligner, bool isFunction)
        {
            if (IsError)
                return "throw new Exception(" + Description.Replace("\"", "\"\"") + ")";

            string statements = new StorageDescriptor(MaxSize.ByteAlignedSize, _frameSize).GetBody(_data,isFunction);
            if(useStatementAligner)
                statements = StatementAligner(statements);
            statements = HWString.Surround("{", statements, "}");
            statements = "fixed(sbyte*data=new sbyte[" + _maxSize.ByteCount + "])" + statements;
            statements = HWString.Indent(statements, 3);
            return statements;
        }

        private bool IsError { get { return _frameSize == null; } }

        private static string StatementAligner(string statements)
        {
            StringAligner aligner = new StringAligner();
            aligner.AddFloatingColumn("frame", "data");
            aligner.AddFloatingColumn(" ", ")");
            aligner.AddFloatingColumn("; // ");
            aligner.AddFloatingColumn(" ");
            return aligner.Format(statements);
        }

        /// <summary>
        /// Gets the max count.
        /// </summary>
        /// <value>The max count.</value>
        /// created 08.10.2006 00:41
        public Size MaxSize { get { return _maxSize; } }

        public static Container UnexpectedVisitOfPending { get { return _unexpectedVisitOfPending; } }
    }

    /// <summary>
    /// Nothing, since void cannot be used for this purpose
    /// </summary>
    public class none
    {
        private static none _instance = new none();

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        /// created 03.10.2006 01:24
        public static none Instance { get { return _instance; } }
    }
}