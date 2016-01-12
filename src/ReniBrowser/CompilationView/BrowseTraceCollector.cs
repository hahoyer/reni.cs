using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using hw.DebugFormatter;
using Reni;
using Reni.Code;

namespace ReniBrowser.CompilationView
{
    sealed class BrowseTraceCollector : DumpableObject, ITraceCollector
    {
        internal sealed class Step : DumpableObject
        {
            internal readonly int Index;
            internal readonly IFormalCodeItem CodeBase;
            internal readonly DataStackMemento Before;
            internal Exception Exception;
            internal DataStackMemento After;

            internal Step(IFormalCodeItem codeBase, DataStackMemento before, int index)
            {
                CodeBase = codeBase;
                Before = before;
                Index = index;
            }

            internal DataGridViewRow CreateRowForStep()
            {
                var result = new DataGridViewRow();
                result.Cells.Add
                    (
                        new DataGridViewTextBoxCell
                        {
                            Value = Index
                        });
                result.Cells.Add
                    (
                        new DataGridViewTextBoxCell
                        {
                            Value = CodeBase.NodeDump()
                        });
                if(Exception != null)
                    result.DefaultCellStyle = new DataGridViewCellStyle
                    {
                        BackColor = Color.Red
                    };
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

            internal void OnSelect(int columnIndex, SourceView master)
            {
                if(columnIndex == 0)
                    master.SignalClickedStep(this);

                if(columnIndex == 1)
                    master.SignalClickedCode(CodeBase);
            }

            internal Control CreateView(SourceView master)
                => false.CreateLineupView
                    (
                        Exception?.ToString().CreateView(),
                        true.CreateLineupView
                            (
                                Before.CreateView(master),
                                After.CreateView(master)
                            )
                    );
        }

        internal sealed class DataStackMemento : DumpableObject
        {
            readonly string Text;

            internal DataStackMemento(DataStack dataStack) { Text = dataStack.Dump(); }

            internal Control CreateView(SourceView master) => Text.CreateView();
        }

        readonly IList<Step> Steps = new List<Step>();
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
            var item = new Step(codeBase, new DataStackMemento(dataStack), index);
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

            item.After = new DataStackMemento(dataStack);
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

        internal ITraceLogItem GetItems(IFormalCodeItem target, SourceView master)
        {
            if(!StepsForCode.ContainsKey(target))
                return EmptyItem;
            var data = StepsForCode[target].Select(item => Steps[item]);
            return new TraceLogItem(data, master);
        }

        static readonly ITraceLogItem EmptyItem = new EmptyTraceLogItem();

        internal DataGridView CreateView(SourceView master)
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
            result.CellClick += (a, b) => Steps[b.RowIndex].OnSelect(b.ColumnIndex, master);
            return result;
        }
    }

    sealed class EmptyTraceLogItem : DumpableObject, ITraceLogItem
    {
        Control ITraceLogItem.CreateLink() => "".CreateView();
    }

    sealed class TraceLogItem : DumpableObject, ITraceLogItem
    {
        readonly SourceView Master;
        readonly BrowseTraceCollector.Step[] Data;

        public TraceLogItem(IEnumerable<BrowseTraceCollector.Step> data, SourceView master)
        {
            Master = master;
            Data = data.ToArray();
        }

        Control ITraceLogItem.CreateLink()
        {
            if(!Data.Any())
                return new Control();

            var text = Data.First().Index.ToString();
            if(Data.Skip(1).Any())
                text += " ...";
            var result = text.CreateView();
            result.Click += (a, b) => OnClick();
            return result;
        }

        void OnClick()
        {
            if(Data.Length == 1)
                Master.SignalClickedStep(Data[0]);
            else
                Master.SignalClickedSteps(Data);
        }

    }
}