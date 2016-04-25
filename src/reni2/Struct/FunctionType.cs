using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using JetBrains.Annotations;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Struct
{
    sealed class FunctionType : SetterTargetType
    {
        [EnableDump]
        internal readonly int Index;
        [DisableDump]
        internal readonly FunctionSyntax Body;
        [Node]
        [EnableDump]
        readonly CompoundView _compoundView;
        [Node]
        internal readonly TypeBase ArgsType;
        [Node]
        [EnableDump]
        internal readonly SetterFunction Setter;
        [NotNull]
        [Node]
        [EnableDump]
        internal readonly GetterFunction Getter;

        internal FunctionType
            (int index, FunctionSyntax body, CompoundView compoundView, TypeBase argsType)
        {
            Getter = new GetterFunction(this, index, body.Getter);
            Setter = body.Setter == null ? null : new SetterFunction(this, index, body.Setter);
            Index = index;
            Body = body;
            _compoundView = compoundView;
            ArgsType = argsType;
            StopByObjectIds();
        }

        protected override bool IsMutable => Setter != null;

        [DisableDump]
        internal override TypeBase ValueType => Getter.ReturnType;
        [DisableDump]
        internal override bool Hllw => Exts.IsNone && ArgsType.Hllw;
        [DisableDump]
        internal override CompoundView FindRecentCompoundView => _compoundView;
        [DisableDump]
        internal override bool HasQuickSize => false;
        internal override IEnumerable<string> DeclarationOptions
            => base.DeclarationOptions.Concat(InternalDeclarationOptions);

        IEnumerable<string> InternalDeclarationOptions
        {
            get
            {
                if(Body.IsImplicit)
                    foreach(var option in ValueType.DeclarationOptions)
                        yield return option;

                yield return "()";

                if (IsMutable)
                    yield return ReassignToken.TokenId;
            }
        }


        [Node]
        [DisableDump]
        internal CodeArgs Exts => GetExts();

        [DisableDump]
        internal FunctionContainer Container
        {
            get
            {
                var getter = Getter.Container;
                var setter = Setter?.Container;
                return new FunctionContainer(getter, setter);
            }
        }

        internal override string DumpPrintText
        {
            get
            {
                var valueType = ValueType;
                var result = "/\\(";
                result += ArgsType.DumpPrintText;
                result += ")=>";
                result += valueType?.DumpPrintText ?? "<unknown>";
                return result;
            }
        }

        [DisableDump]
        internal IEnumerable<IFormalCodeItem> CodeItems
        {
            get
            {
                var getter = Getter.CodeItems;
                var setter = Setter?.CodeItems;
                return setter == null ? getter : getter.Concat(setter);
            }
        }

        protected override Result SetterResult(Category category) => Setter.GetCallResult(category);
        protected override Result GetterResult(Category category) => Getter.GetCallResult(category);
        protected override Size GetSize() => ArgsType.Size + Exts.Size;

        internal ContextBase CreateSubContext(bool useValue)
            => new Context.Function(_compoundView.Context, ArgsType, useValue ? ValueType : null);

        public string DumpFunction()
        {
            var result = "\n";
            result += "index=" + Index;
            result += "\n";
            result += "argsType=" + ArgsType.Dump();
            result += "\n";
            result += "context=" + _compoundView.Dump();
            result += "\n";
            result += "Getter=" + Getter.DumpFunction();
            result += "\n";
            if(Setter != null)
            {
                result += "Setter=" + Setter.DumpFunction();
                result += "\n";
            }
            result += "type=" + ValueType.Dump();
            result += "\n";
            return result;
        }

        // remark: watch out when caching anything here. 
        // This may hinder the recursive call detection, located at result cache of context. 
        public Result ApplyResult(Category category)
        {
            var trace = Index.In();
            StartMethodDump(trace, category);
            try
            {
                BreakExecution();
                var result = Result
                    (
                        category,
                        () => Exts.ToCode() + ArgsType.ArgCode,
                        () => Exts + CodeArgs.Arg()
                    );
                Tracer.Assert(category == result.CompleteCategory);
                return ReturnMethodDump(result);
            }
            finally
            {
                EndMethodDump();
            }
        }

        CodeArgs GetExts()
        {
            var result = Getter.Exts;
            Tracer.Assert(result != null);
            if(Setter != null)
                result += Setter.Exts;
            var argsExt = ArgsType as IContextReference;
            if(argsExt != null)
                Tracer.Assert(!result.Contains(argsExt));
            return result;
        }
    }
}