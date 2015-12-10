using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Windows.Forms;
using hw.Helper;
using hw.Scanner;
using Reni.Context;
using Reni.Struct;
using Reni.Type;

namespace ReniTest.CompilationView
{
    sealed class TypeView : ChildView
    {
        public TypeView(TypeBase item, SourceView master)
            : base(master, item.NodeDump)
        {
            Client = item.CreateView(Master);
            Source = item.GetSource();
        }

        protected override SourcePart Source { get; }
    }
}