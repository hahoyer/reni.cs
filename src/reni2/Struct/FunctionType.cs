using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Forms;
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
        readonly int _index;
        [Node]
        [EnableDump]
        readonly CompoundView _compoundView;
        [Node]
        internal readonly TypeBase ArgsType;
        [Node]
        [EnableDump]
        readonly SetterFunction _setter;
        [NotNull]
        [Node]
        [EnableDump]
        readonly GetterFunction _getter;

        internal FunctionType(int index, FunctionSyntax body, CompoundView compoundView, TypeBase argsType)
        {
            _getter = new GetterFunction(this, index, body.Getter);
            _setter = body.Setter == null ? null : new SetterFunction(this, index, body.Setter);
            _index = index;
            _compoundView = compoundView;
            ArgsType = argsType;
            StopByObjectId(-10);
        }

        protected override bool IsMutable => _setter != null;
        [DisableDump]
        protected override TypeBase Source => _compoundView.Type;

        [DisableDump]
        internal override TypeBase ValueType => _getter.ReturnType;
        [DisableDump]
        internal override bool Hllw => Exts.IsNone && ArgsType.Hllw;
        [DisableDump]
        internal override CompoundView FindRecentCompoundView => _compoundView;
        [DisableDump]
        internal override bool HasQuickSize => false;

        [Node]
        [DisableDump]
        CodeArgs Exts => GetExts();

        [DisableDump]
        internal FunctionContainer Container
        {
            get
            {
                var getter = _getter.Container;
                var setter = _setter?.Container;
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
        protected override Result SetterResult(Category category) => _setter.CallResult(category);
        protected override Result GetterResult(Category category) => _getter.CallResult(category);
        protected override Size GetSize() => ArgsType.Size + Exts.Size;

        internal ContextBase CreateSubContext(bool useValue)
            => new Context.Function(_compoundView.Context, ArgsType, useValue ? ValueType : null);

        public string DumpFunction()
        {
            var result = "\n";
            result += "index=" + _index;
            result += "\n";
            result += "argsType=" + ArgsType.Dump();
            result += "\n";
            result += "context=" + _compoundView.Dump();
            result += "\n";
            result += "Getter=" + _getter.DumpFunction();
            result += "\n";
            if(_setter != null)
            {
                result += "Setter=" + _setter.DumpFunction();
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
            var result = Result
                (
                    category,
                    () => Exts.ToCode() + ArgsType.ArgCode,
                    () => Exts + CodeArgs.Arg()
                );
            Tracer.Assert(category == result.CompleteCategory);
            return result;
        }

        CodeArgs GetExts()
        {
            var result = _getter.Exts;
            Tracer.Assert(result != null);
            if(_setter != null)
                result += _setter.Exts;
            return result;
        }
    }
}