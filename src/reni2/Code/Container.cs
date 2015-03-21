using System;
using System.Collections.Generic;
using hw.Helper;
using System.Linq;
using hw.Debug;
using hw.Forms;
using Reni.Basics;
using Reni.Struct;
using Reni.Validation;

namespace Reni.Code
{
    sealed class Container : DumpableObject
    {
        static int _nextObjectId;
        static readonly Container _unexpectedVisitOfPending = new Container
            ("UnexpectedVisitOfPending");

        readonly string _description;
        readonly CodeBase _data;
        [Node]
        internal readonly FunctionId FunctionId;

        public Container(CodeBase data, string description, FunctionId functionId = null)
            : base(_nextObjectId++)
        {
            _description = description;
            FunctionId = functionId;
            _data = data;
            StopByObjectId(-10);
        }

        Container(string errorText) { _description = errorText; }

        [Node]
        [EnableDump]
        internal CodeBase Data => _data;

        [Node]
        [EnableDump]
        internal string Description => _description;

        [Node]
        [DisableDump]
        public Size MaxSize => _data.TemporarySize;

        [Node]
        [DisableDump]
        public static Container UnexpectedVisitOfPending => _unexpectedVisitOfPending;
        [Node]
        public Issue[] Issues => _data.Issues.ToArray();

        public string GetCSharpStatements(int indent)
        {
            var generator = new CSharpGenerator(_data.TemporarySize.SaveByteCount);
            try
            {
                _data.Visit(generator);
            }
            catch(UnexpectedContextReference e)
            {
                Tracer.AssertionFailed("", () => e.Message);
            }
            return generator.Data.Indent(indent);
        }
    }
}