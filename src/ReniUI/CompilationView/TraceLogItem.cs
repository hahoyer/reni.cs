using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using hw.DebugFormatter;

namespace ReniUI.CompilationView
{
    sealed class TraceLogItem : DumpableObject, ITraceLogItem
    {
        readonly SourceView Master;
        readonly Step[] Data;

        public TraceLogItem(IEnumerable<Step> data, SourceView master)
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
            Master.SignalClickedSteps(Data);
            if (Data.Length == 1)
                Master.SignalClickedStep(Data[0]);
        }
    }
}