using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using hw.DebugFormatter;
using hw.Helper;
using JetBrains.Annotations;
using Reni;
using Reni.Code;


namespace ReniTest.CompilationView
{
    sealed class BrowseTraceCollector : DumpableObject, ITraceCollector
    {
        internal sealed class Step : DumpableObject
        {
            [UsedImplicitly]
            internal int Index;
            internal readonly IFormalCodeItem CodeBase;
            readonly DataStackMemento Before;
            internal readonly Exception Exception;
            readonly DataStackMemento After;

            public Step
                (
                IFormalCodeItem codeBase,
                DataStackMemento before,
                Exception exception,
                DataStackMemento after,
                int index)
            {
                CodeBase = codeBase;
                Before = before;
                Exception = exception;
                After = after;
                Index = index;
            }

            internal DataGridViewRow CreateRowForStep()
            {
                var result = new DataGridViewRow();
                result.Cells.Add(new DataGridViewTextBoxCell {Value = Index});
                result.Cells.Add(new DataGridViewTextBoxCell {Value = CodeBase.NodeDump()});
                if(Exception != null)
                    result.DefaultCellStyle = new DataGridViewCellStyle {BackColor = Color.Red};
                return result;
            }

            internal static IEnumerable<DataGridViewColumn> DataGridViewColumns()
            {
                yield return new DataGridViewTextBoxColumn
                {
                    Name = "#",
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                    DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Alignment = DataGridViewContentAlignment.MiddleRight
                    }
                };
                yield return new DataGridViewTextBoxColumn
                {
                    Name = "Code",
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
                };
            }

            internal void OnEnter(int columnIndex, SourceView master)
            {
                switch (columnIndex)
                {
                case 0:
                    CreateView(Before);
                    return;
                case 1:
                    if (Exception == null)
                        CreateView(After);
                    else
                        CreateView(Exception);
                    return;
                }
            }

            internal void OnSelect(int columnIndex, SourceView master)
            {
                switch (columnIndex)
                {
                case 0:
                    return;
                case 1:
                    master.    CreateView(Exception);
                    return;
                }
            }

            void CreateView(Exception target) { NotImplementedMethod(target); }

            void CreateView(DataStackMemento taget) { NotImplementedMethod(taget); }
        }

        internal sealed class DataStackMemento : DumpableObject
        {
            readonly string Text;
            public DataStackMemento(DataStack dataStack) { Text = dataStack.Dump(); }
        }

        readonly IList<Step> Steps = new List<Step>();
        readonly IDictionary<IFormalCodeItem, int[]> StepsForCode = new Dictionary<IFormalCodeItem, int[]>();

        void ITraceCollector.AssertionFailed(Func<string> dumper, int depth)
        {
            throw new AssertionFailedException(dumper());
        }

        void ITraceCollector.Run(DataStack dataStack, IFormalCodeItem codeBase)
        {
            var before = new DataStackMemento(dataStack);
            Exception runException = null;
            var beforeSize = dataStack.Size;
            try
            {
                codeBase.Visit(dataStack);
            }
            catch(Exception exception)
            {
                runException = exception;
                dataStack.Size = beforeSize + codeBase.Size;
            }

            var index = Steps.Count;
            var item = new Step(codeBase, before, runException, new DataStackMemento(dataStack), index);
            AssumeStepsForCode(item.CodeBase, index);

            Steps.Add(item);
        }

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
            return new TraceLogItem(data);
        }

        static readonly ITraceLogItem EmptyItem = new EmptyTraceLogItem();

        internal Control CreateView(SourceView master)
        {
            var result = new DataGridView {RowHeadersVisible = false};
            result.Columns.AddRange(Step.DataGridViewColumns().ToArray());
            result.Rows.AddRange(Steps.Select(item => item.CreateRowForStep()).ToArray());
            result.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
            result.CellDoubleClick += (a, b) => Steps[b.RowIndex].OnEnter(b.ColumnIndex, master);
            return result;
        }
    }

    class EmptyTraceLogItem : DumpableObject, ITraceLogItem
    {
        Control ITraceLogItem.CreateLink() => "".CreateView();
    }

    class TraceLogItem : DumpableObject, ITraceLogItem
    {
        readonly BrowseTraceCollector.Step[] Data;
        public TraceLogItem(IEnumerable<BrowseTraceCollector.Step> data) { Data = data.ToArray(); }

        Control ITraceLogItem.CreateLink()
        {
            var result = Data
                .Select(item => item.Index + (item.Exception == null ? "" : "?"))
                .Stringify(", ")
                .CreateView();
            result.Click += (a, b) => OnClick();
            return result;
        }

        void OnClick() { NotImplementedMethod(); }
    }
}