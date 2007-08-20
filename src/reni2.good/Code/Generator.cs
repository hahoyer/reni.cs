using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HWClassLibrary.Debug;
using Microsoft.CSharp;
using File=HWClassLibrary.IO.File;

namespace Reni.Code
{
    static class Generator
    {
        static string _baseName = "";
        static int _nextId = 0;
        static readonly CSharpCodeProvider _provider = new CSharpCodeProvider();
        public static string NotACommentFlag { get { return "<notacomment> "; } }

        /// <summary>
        /// Gets the name of the main method.
        /// </summary>
        /// <value>The name of the main method.</value>
        /// created 15.11.2006 22:04
        public static string MainMethodName { get { return "reni"; } }
        /// <summary>
        /// Functions the name of the method.
        /// </summary>
        /// <param name="i">The i.</param>
        /// <returns></returns>
        /// created 15.11.2006 22:04
        public static string FunctionMethodName(int i){return "reni_"+i;}

        /// <summary>
        /// Creates the C sharp code.
        /// </summary>
        /// <param name="main">The main.</param>
        /// <param name="functions">The functions.</param>
        /// <param name="align">if set to <c>true</c> [align].</param>
        /// <returns></returns>
        /// created 08.10.2006 02:35
        public static string CreateCSharpString(Container main, List<Container> functions, bool align)
        {
            return CodeToString(CreateCompileUnit(main, functions, align));
        }

        /// <summary>
        /// Creates the C sharp assembly.
        /// </summary>
        /// <param name="main">The main.</param>
        /// <param name="functions">The functions.</param>
        /// <param name="align">if set to <c>true</c> [align].</param>
        /// <returns></returns>
        /// created 08.10.2006 22:44
        public static Assembly CreateCSharpAssembly(Container main, List<Container> functions, bool align)
        {
            return CodeToAssembly(CreateCompileUnit(main, functions,align));
        }

        private static CodeCompileUnit CreateCompileUnit(Container main, List<Container> functions, bool align)
        {
            string name = CreateName();
            CodeTypeDeclaration ctd = main.GetCSharpTypeCode(functions, name, align);
            return CreateCSharpCode(ctd, name);
        }

        private static CodeCompileUnit CreateCSharpCode(CodeTypeDeclaration ctd, string name)
        {
            return ToCompileUnit(ToNameSpace(ctd, name));
        }

        private static CodeCompileUnit ToCompileUnit(CodeNamespace ns)
        {
            CodeCompileUnit cu = new CodeCompileUnit();
            cu.Namespaces.Add(ns);
            cu.ReferencedAssemblies.Add("reni.dll");
            cu.ReferencedAssemblies.Add("HWClassLibrary.dll");
            return cu;
        }

        private static CodeNamespace ToNameSpace(CodeTypeDeclaration ctd, string name)
        {
            CodeNamespace ns = new CodeNamespace(name);
            ns.Types.Add(ctd);
            ns.Imports.Add(new CodeNamespaceImport("System"));
            ns.Imports.Add(new CodeNamespaceImport("Reni"));
            ns.Imports.Add(new CodeNamespaceImport("Reni.Runtime"));
            
            return ns;
        }

        private static string CodeToString(CodeCompileUnit cu)
        {
            StringWriter sw = new StringWriter();
            CodeToText(sw,cu);
            string result = sw.ToString().Replace("// "+NotACommentFlag, "");
            return result;
        }

        private static void CodeToFile(string name, CodeCompileUnit cu)
        {
            StreamWriter sw = new StreamWriter(name);
            Tracer.Line(Tracer.FilePosn(File.m(name).FullName,0,0,""));
            sw.Write(CodeToString(cu));
            sw.Close();
        }

        private static void CodeToText(TextWriter tw, CodeCompileUnit cu)
        {
            CodeGeneratorOptions codeGeneratorOptions = new CodeGeneratorOptions();
            codeGeneratorOptions.BlankLinesBetweenMembers = true;
            _provider.GenerateCodeFromCompileUnit(cu, tw, codeGeneratorOptions);
        }

        public static Assembly CodeToAssembly(CodeCompileUnit cu)
        {
            string name = "generated.cs";
            CodeToFile(name,cu);
            
            // Build the parameters for source compilation.
            System.CodeDom.Compiler.CompilerParameters cp = new System.CodeDom.Compiler.CompilerParameters();
            cp.GenerateInMemory = true;
            cp.CompilerOptions = "/unsafe";
            cp.IncludeDebugInformation = true;
            cp.ReferencedAssemblies.AddRange(GetReferencesAssemblies(cu));
            CompilerResults cr = _provider.CompileAssemblyFromFile(cp, name);

            if (cr.Errors.Count > 0)
                HandleErrors(cr.Errors);

            return cr.CompiledAssembly;

        }

        private static string[] GetReferencesAssemblies(CodeCompileUnit cu)
        {
            string[] result = new string[cu.ReferencedAssemblies.Count];
            cu.ReferencedAssemblies.CopyTo(result, 0);
            return result;
        }

        private static void HandleErrors(CompilerErrorCollection cr)
        {
            for (int i = 0; i < cr.Count; i++ )
                Tracer.Line(cr[i].ToString());
            
            throw new CompilerErrorException(cr);
        }

        /// <summary>
        /// Creates the name of the class.
        /// </summary>
        /// <returns></returns>
        /// created 08.10.2006 02:35
        static string CreateName()
        {
            if (_baseName == "")
                _baseName = CreateBaseName();

            string result = _baseName;
            int id = _nextId++;
            if(id != 0)
            {
                result += "_";
                result += id;
            }
            return result;
        }

        static string CreateBaseName()
        {
            return Compiler.FormattedNow;
        }
    }

    internal class CompilerErrorException : Exception
    {
        private readonly CompilerErrorCollection _compilerErrorCollection;

        public CompilerErrorCollection CompilerErrorCollection { get { return _compilerErrorCollection; } }

        public CompilerErrorException(CompilerErrorCollection cr)
        {
            _compilerErrorCollection = cr;
        }
    }
}
