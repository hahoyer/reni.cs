using System.Reflection;
using System.Windows.Forms;
using HWClassLibrary.Helper.TreeViewSupport;
using HWClassLibrary.IO;
using Reni;


namespace ReniTest
{
    public partial class Main : Form
    {
        private Reni.Compiler _compiler;

        public Main()
        {
            InitializeComponent();
            CreateCompiler(@"i: 400; f: function i > 0 then (i := i - 1; f());f()");
            treeView1.Nodes.Add(Service.CreateNode("Compiler", _compiler, 20));
        }

        private void CreateCompiler(string text)
        {
            string fileName = "temptest.reni";
            File f = File.m(fileName);
            f.String = text;
            _compiler = new Compiler(fileName);
            _compiler.Exec();
        }
    }

}