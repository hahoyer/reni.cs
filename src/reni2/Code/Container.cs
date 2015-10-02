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

        [Node]
        internal readonly FunctionId FunctionId;

        public Container(CodeBase data, string description, FunctionId functionId = null)
            : base(_nextObjectId++)
        {
            Description = description;
            FunctionId = functionId;
            Data = data;
            Tracer.Assert(Data.Exts.IsNone);
            StopByObjectIds(-10);
        }

        Container(string errorText) { Description = errorText; }

        [Node]
        [EnableDump]
        internal CodeBase Data { get; }

        [Node]
        [EnableDump]
        internal string Description { get; }

        [Node]
        [DisableDump]
        public Size MaxSize => Data.TemporarySize;

        [Node]
        [DisableDump]
        public static Container UnexpectedVisitOfPending { get; }
            = new Container("UnexpectedVisitOfPending");

        [Node]
        public Issue[] Issues => Data.Issues.ToArray();

        public string GetCSharpStatements(int indent)
        {
            var generator = new CSharpGenerator(Data.TemporarySize.SaveByteCount);
            Data.Visit(generator);
            return generator.Data.Indent(indent);
        }
    }
}