using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using hw.DebugFormatter;
using Reni.Code;
using Reni.Struct;

namespace ReniUI.CompilationView
{
    sealed class BrowseTraceCollector : DumpableObject, ITraceCollector
    {
        readonly IList<Step> Steps = new List<Step>();
        readonly IList<FrameItem> Frames = new List<FrameItem>();
        readonly SourceView SourceView;

        internal BrowseTraceCollector(SourceView sourceView) { SourceView = sourceView; }


        readonly IDictionary<IFormalCodeItem, int[]> StepsForCode =
            new Dictionary<IFormalCodeItem, int[]>();

        void ITraceCollector.AssertionFailed(Func<string> dumper, int depth)
        {
            throw new AssertionFailedException(dumper());
        }

        void ITraceCollector.Run(DataStack dataStack, IFormalCodeItem codeBase)
        {
            var beforeSize = dataStack.Size;
            var index = Steps.Count;
            var item = new Step(codeBase, dataStack, index, Frames.ToArray());
            Steps.Add(item);
            AssumeStepsForCode(item.CodeBase, index);

            try
            {
                codeBase.Visit(dataStack);
            }
            catch(Exception exception)
            {
                item.Exception = exception;
                dataStack.Size = beforeSize + codeBase.Size;
            }

            item.AfterStack = dataStack;
        }

        void ITraceCollector.Call(StackData argsAndRefs, FunctionId functionId)
            => Frames.Insert
                (
                    0,
                    new FrameItem
                    {
                        Text = argsAndRefs.Dump(),
                        FunctionId = functionId,
                        CallStep = new TraceLogItem(new[] {Steps.Last()}, SourceView)
                    });

        void ITraceCollector.Return() => Frames.RemoveAt(0);

        void AssumeStepsForCode(IFormalCodeItem item, int index)
        {
            if(!StepsForCode.ContainsKey(item))
                StepsForCode.Add(item, new int[0]);
            StepsForCode[item] = StepsForCode[item].Concat(new[] {index}).ToArray();
        }

        internal abstract class RunException : Exception
        {
            protected RunException(string message)
                : base(message) {}
        }

        internal sealed class AssertionFailedException : RunException
        {
            public AssertionFailedException(string message)
                : base(message) {}
        }

        internal ITraceLogItem GetItems(IFormalCodeItem target)
        {
            if(!StepsForCode.ContainsKey(target))
                return EmptyItem;
            var data = StepsForCode[target].Select(item => Steps[item]);
            return new TraceLogItem(data, SourceView);
        }

        static readonly ITraceLogItem EmptyItem = new EmptyTraceLogItem();

        internal DataGridView CreateView()
        {
            var result = new DataGridView
            {
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.RowHeaderSelect,
                MultiSelect = true
            };
            result.Columns.AddRange(Step.DataGridViewColumns().ToArray());
            result.Rows.AddRange(Steps.Select(item => item.CreateRowForStep()).ToArray());
            result.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
            result.CellClick += (a, b) => Steps[b.RowIndex].OnSelect(b.ColumnIndex, SourceView);
            return result;
        }
    }
}