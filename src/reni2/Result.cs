//     Compiler for programming language "Reni"
//     Copyright (C) 2011 Harald Hoyer
// 
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//     
//     Comments, bugs and suggestions to hahoyer at yahoo.de

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Struct;
using Reni.Syntax;
using Reni.Type;

namespace Reni
{
    [Serializable]
    internal sealed class Result : ReniObject, ITreeNodeSupport
    {
        private static int _nextObjectId = 1;
        private bool _isDirty;
        private bool? _isDataLess;
        private Size _size;
        private TypeBase _type;
        private CodeBase _code;
        private CodeArgs _codeArgs;

        internal Result()
            : base(_nextObjectId++)
        {
            PendingCategory = new Category();
            StopByObjectId(-135);
        }

        internal Result(Category category, Func<bool> getDataLess, Func<Size> getSize, Func<TypeBase> getType, Func<CodeBase> getCode, Func<CodeArgs> getRefs)
            : this()
        {
            if(category.HasSize)
                _size = getSize();
            if(category.HasType)
                _type = getType();
            if(category.HasCode)
                _code = getCode();
            if(category.HasArgs)
                _codeArgs = getRefs();
            if(category.HasIsDataLess)
                _isDataLess = getDataLess();
            AssertValid();
        }

        internal Result(Category category, Func<bool> getDataLess, Func<Size> getSize, Func<CodeBase> getCode, Func<CodeArgs> getRefs)
            : this()
        {
            if(category.HasSize)
                _size = getSize();
            if(category.HasCode)
                _code = getCode();
            if(category.HasArgs)
                _codeArgs = getRefs();
            if(category.HasIsDataLess)
                _isDataLess = getDataLess();
            AssertValid();
        }

        internal Result(Category category, Func<CodeArgs> getRefs)
            : this(category, () => true, () => Size.Zero, CodeBase.Void, getRefs) { }

        private bool HasSize { get { return Size != null; } }
        internal bool HasType { get { return Type != null; } }
        internal bool HasCode { get { return Code != null; } }
        internal bool HasArgs { get { return CodeArgs != null; } }
        internal bool HasIsDataLess { get { return _isDataLess != null; } }

        [Node]
        [EnableDumpWithExceptions]
        internal Category PendingCategory;

        public Category CompleteCategory { get { return new Category(HasIsDataLess, HasSize, HasType, HasCode, HasArgs); } }

        [Node]
        [DebuggerHidden]
        public bool? IsDataLess
        {
            get { return _isDataLess; }
            set
            {
                _isDataLess = value;
                AssertValid();
            }
        }

        [Node]
        [DebuggerHidden]
        public Size Size
        {
            get { return _size; }
            set
            {
                _size = value;
                AssertValid();
            }
        }

        [Node]
        [DebuggerHidden]
        public TypeBase Type
        {
            get { return _type; }
            set
            {
                _type = value;
                AssertValid();
            }
        }

        [Node]
        internal CodeBase Code
        {
            [DebuggerHidden]
            get { return _code; }
            [DebuggerHidden]
            set
            {
                _code = value;
                AssertValid();
            }
        }

        [Node]
        [DebuggerHidden]
        public CodeArgs CodeArgs
        {
            get { return _codeArgs; }
            set
            {
                _codeArgs = value;
                AssertValid();
            }
        }

        TreeNode[] ITreeNodeSupport.CreateNodes()
        {
            var result = new List<TreeNode>();
            if(PendingCategory.HasAny)
                result.Add(Dump().CreateNamedNode("Pending", "Pending"));
            if(HasIsDataLess)
                result.Add(IsDataLess.CreateNamedNode("IsDataLess", "Logical"));
            if(HasSize)
                result.Add(Size.FormatForView().CreateNamedNode("Size", "Number"));
            if(HasType)
                result.Add(Type.CreateNamedNode("Type", "Type"));
            if(HasCode)
                result.Add(Code.CreateNamedNode("Code", "Code"));
            if(HasArgs)
                result.Add(CodeArgs.Data.CreateNamedNode("Args", "Args"));
            return result.ToArray();
        }

        internal bool? FindIsDataLess
        {
            get
            {
                if (HasIsDataLess)
                    return _isDataLess;
                var size = FindSize;
                if(size == null)
                    return null;
                return size.IsZero;
            }
        }

        internal bool SmartIsDataLess
        {
            get
            {
                var result = FindIsDataLess;
                if(result == null)
                {
                    DumpMethodWithBreak("No approriate result property defined");
                    Debugger.Break();
                    return false;
                }
                return result.Value;
            }
        }

        internal Size FindSize
        {
            get
            {
                if(HasSize)
                    return Size;
                if(HasCode)
                    return Code.Size;
                if(HasType)
                    return Type.Size;
                return null;
            }
        }

        internal Size SmartSize
        {
            get
            {
                var result = FindSize;
                if(result == null)
                {
                    DumpMethodWithBreak("No approriate result property defined");
                    Debugger.Break();
                }
                return result;
            }
        }

        internal CodeArgs FindArgs
        {
            get
            {
                if(HasArgs)
                    return CodeArgs;
                if(HasCode)
                    return Code.CodeArgs;
                return null;
            }
        }

        [DisableDump]
        internal CodeArgs SmartArgs
        {
            get
            {
                var result = FindArgs;
                if(result == null)
                {
                    DumpMethodWithBreak("No approriate result property defined");
                    Debugger.Break();
                }
                return result;
            }
        }

        internal static Error Error { get { return null; } }

        private bool IsDirty
        {
            get { return _isDirty; }
            set
            {
                _isDirty = value;
                AssertValid();
            }
        }

        internal bool IsEmpty
        {
            get
            {
                if(HasIsDataLess && IsDataLess == false)
                    return false;
                if(HasSize && !Size.IsZero)
                    return false;
                if(HasType && !(Type is Type.Void))
                    return false;
                if(HasCode && !Code.IsEmpty)
                    return false;
                if(HasArgs && !CodeArgs.IsNone)
                    return false;
                return true;
            }
        }

        internal bool IsCodeLess
        {
            get
            {
                if(CompleteCategory.HasSize && !Size.IsZero)
                    return false;
                if(CompleteCategory.HasCode && !Code.IsEmpty)
                    return false;
                if(CompleteCategory.HasArgs && !CodeArgs.IsNone)
                    return false;
                return true;
            }
        }

        internal bool HasArg
        {
            get
            {
                if(HasArgs)
                    return CodeArgs.HasArg;
                if(HasCode)
                    return Code.HasArg;
                return false;
            }
        }


        public override string DumpData()
        {
            var result = "";
            result += "PendingCategory=" + PendingCategory.Dump();
            result += "\n";
            result += "CompleteCategory=" + CompleteCategory.Dump();
            if(HasIsDataLess)
                result += "\nIsDataLess=" + Tracer.Dump(_isDataLess);
            if(HasSize)
                result += "\nSize=" + Tracer.Dump(_size);
            if(HasType)
                result += "\nType=" + Tracer.Dump(_type);
            if(HasArgs)
                result += "\nRefs=" + Tracer.Dump(_codeArgs);
            if(HasCode)
                result += "\nCode=" + Tracer.Dump(_code);
            return result;
        }

        internal void Update(Result result)
        {
            if(result.HasIsDataLess)
                _isDataLess = result.IsDataLess;

            if(result.HasSize)
                _size = result.Size;

            if(result.HasType)
                _type = result.Type;

            if(result.HasArgs)
                _codeArgs = result.CodeArgs;

            if(result.HasCode)
                _code = result.Code;

            AssertValid();
        }

        private Result Filter(Category category)
        {
            var result = new Result
                         {
                             PendingCategory = PendingCategory & category
                         };

            if(category.HasIsDataLess)
                result._isDataLess = IsDataLess;
            if(category.HasSize)
                result._size = Size;
            if(category.HasType)
                result._type = Type;
            if(category.HasArgs)
                result._codeArgs = CodeArgs;
            if(category.HasCode)
                result._code = Code;
            result.AssertValid();
            return result;
        }

        internal Result Align(int alignBits)
        {
            var size = FindSize;
            if(size == null)
                return this;

            var alignedSize = size.Align(alignBits);
            if(alignedSize == size)
                return this;

            var result = new Result();
            if(HasIsDataLess)
                result._isDataLess = IsDataLess;
            if(HasSize)
                result.Size = alignedSize;
            if(HasType)
                result.Type = Type.UniqueAlign(alignBits);
            if(HasCode)
                result.Code = Code.BitCast(alignedSize);
            if(HasArgs)
                result.CodeArgs = CodeArgs;
            return result;
        }

        private Result Clone(Category category)
        {
            var result = new Result {PendingCategory = PendingCategory & category};
            if(category.HasIsDataLess)
                result._isDataLess = IsDataLess;
            if(category.HasSize)
                result.Size = Size;
            if(category.HasType)
                result.Type = Type;
            if(category.HasCode)
                result.Code = Code;
            if(category.HasArgs)
                result.CodeArgs = CodeArgs;
            return result;
        }

        internal Result Clone() { return new Result {PendingCategory = PendingCategory, IsDataLess = IsDataLess, Size = Size, Type = Type, Code = Code, CodeArgs = CodeArgs}; }

        private void AssertValid()
        {
            if(IsDirty)
                return;

            var isDataLess = FindIsDataLess;
            if(isDataLess != null)
            {
                if(HasIsDataLess && IsDataLess != isDataLess.Value)
                    Tracer.AssertionFailed(1, @"IsDataLess==isDataLess", () => "IsDataLess differs " + Dump());
                if(HasSize && Size.IsZero != isDataLess.Value)
                    Tracer.AssertionFailed(1, @"Size.IsZero==isDataLess.Value", () => "Size differs " + Dump());
                if(HasType && Type.Size.IsZero != isDataLess.Value)
                    Tracer.AssertionFailed(1, @"Type.Size.IsZero==isDataLess.Value", () => "Type size differs " + Dump());
                if(HasCode && Code.Size.IsZero != isDataLess.Value)
                    Tracer.AssertionFailed(1, @"Code.Size.IsZero==isDataLess.Value", () => "Code size differs " + Dump());
            }

            var size = FindSize;
            if(size != null)
            {
                if(HasSize && Size != size)
                    Tracer.AssertionFailed(1, @"Size==size", () => "Size differs " + Dump());
                if(HasType && Type.Size != size)
                    Tracer.AssertionFailed(1, @"Type.Size==size", () => "Type size differs " + Dump());
                if(HasCode && Code.Size != size)
                    Tracer.AssertionFailed(1, @"Code.Size==size", () => "Code size differs " + Dump());
            }

            if(HasArgs && HasCode)
            {
                var refs = CodeArgs;
                var codeRefs = Code.CodeArgs;
                if(!(refs.Contains(codeRefs) && codeRefs.Contains(refs)))
                    Tracer.AssertionFailed(1, @"Args.Contains(codeRefs)", () => "Code and Args differ " + Dump());
            }
        }

        internal void AssertComplete(Category category, CompileSyntax syntaxForDump)
        {
            Tracer.Assert
                (
                    1,
                    category <= (CompleteCategory | PendingCategory),
                    () => string.Format("syntax={2}\ncategory={0}\nResult={1}", category.DumpShort(), Dump(), syntaxForDump.DumpShort())
                );
        }

        private void Add(Result other) { Add(other, CompleteCategory); }

        private void Add(Result other, Category category)
        {
            Tracer.Assert(category <= other.CompleteCategory);
            Tracer.Assert(category <= CompleteCategory);
            IsDirty = true;
            if(category.HasIsDataLess)
                IsDataLess = SmartIsDataLess && other.SmartIsDataLess;
            if(category.HasSize)
                Size += other.Size;
            if(category.HasType)
                Type = Type.Pair(other.Type);
            if(category.HasCode)
                Code = Code.Sequence(other.Code);
            if(category.HasArgs)
                CodeArgs = CodeArgs.Sequence(other.CodeArgs);
            IsDirty = false;
        }

        internal Result Sequence(Result second)
        {
            var result = Clone();
            result.Add(second);
            return result;
        }

        internal Result ReplaceArg(Func<Result> getResultForArg)
        {
            if(HasArg)
                return InternalReplaceArg(getResultForArg());
            return this;
        }

        internal Result ReplaceArg(Result resultForArg)
        {
            if(HasArg)
                return InternalReplaceArg(resultForArg);
            return this;
        }

        private Result InternalReplaceArg(Result resultForArg)
        {
            var result = new Result {IsDataLess = IsDataLess, Size = Size, Type = Type, IsDirty = true};
            if(HasCode && resultForArg.HasCode)
                result.Code = Code.ReplaceArg(resultForArg.Type, resultForArg.Code);
            if(HasArgs && resultForArg.HasArgs)
                result.CodeArgs = CodeArgs.WithoutArg().Sequence(resultForArg.CodeArgs);
            result.IsDirty = false;
            return result;
        }

        internal Result ReplaceAbsolute<TRefInCode>(TRefInCode refInCode, Func<CodeBase> replacementCode, Func<CodeArgs> replacementRefs)
            where TRefInCode : IReferenceInCode
        {
            if(HasArgs && !CodeArgs.Contains(refInCode))
                return this;

            var result = new Result {IsDataLess = IsDataLess, Size = Size, Type = Type, IsDirty = true};
            if(HasCode)
                result.Code = Code.ReplaceAbsolute(refInCode, replacementCode);
            if(HasArgs)
                result.CodeArgs = CodeArgs.Without(refInCode).Sequence(replacementRefs());
            result.IsDirty = false;
            return result;
        }

        internal Result ReplaceAbsolute<TRefInCode>(TRefInCode refInCode, Func<Category, Result> getReplacement)
            where TRefInCode : IReferenceInCode
        {
            if(HasArgs && !CodeArgs.Contains(refInCode))
                return this;

            var replacement = getReplacement(CompleteCategory - Category.Size - Category.Type);
            var result = new Result {IsDataLess = IsDataLess, Size = Size, Type = Type, IsDirty = true};
            if(HasCode)
                result.Code = Code.ReplaceAbsolute(refInCode, () => replacement.Code);
            if(HasArgs)
                result.CodeArgs = CodeArgs.Without(refInCode).Sequence(replacement.CodeArgs);
            result.IsDirty = false;
            return result;
        }

        internal Result ReplaceRelative<TRefInCode>(TRefInCode refInCode, Func<CodeBase> replacementCode, Func<CodeArgs> replacementRefs)
            where TRefInCode : IReferenceInCode
        {
            if(HasArgs && !CodeArgs.Contains(refInCode))
                return this;

            var result = new Result {IsDataLess = IsDataLess, Size = Size, Type = Type};
            if(HasCode)
                result.Code = Code.ReplaceRelative(refInCode, replacementCode);
            if(HasArgs)
                result.CodeArgs = CodeArgs.Without(refInCode).Sequence(replacementRefs());
            return result;
        }

        internal Result ReplaceRefsForFunctionBody(RefAlignParam refAlignParam, CodeBase replacement)
        {
            if(!HasCode)
                return this;
            if(SmartArgs.Count == 0)
                return this;
            var result = Clone();
            result.IsDirty = true;
            result.Code = SmartArgs.ReplaceRefsForFunctionBody(Code, refAlignParam, replacement);
            result.CodeArgs = replacement.CodeArgs;
            result.IsDirty = false;
            return result;
        }

        internal Result LocalBlock(Category category)
        {
            if(!category.HasCode && !category.HasArgs)
                return this;

            var result = Clone(category);
            var copier = Type.Copier(category);
            if(category.HasCode)
                result.Code = Code.LocalBlock(copier.Code);
            if(category.HasArgs)
                result.CodeArgs = CodeArgs.Sequence(copier.CodeArgs);
            return result;
        }

        internal Result Conversion(TypeBase target) { return Type.Conversion(CompleteCategory, target).ReplaceArg(this); }

        internal BitsConst Evaluate()
        {
            Tracer.Assert(CodeArgs.IsNone);
            var result = Align(3).LocalBlock(CompleteCategory);
            return result.Code.Evaluate();
        }

        internal Result AutomaticDereference()
        {
            if(CompleteCategory == Category.Args)
                return this;

            Tracer.Assert(HasType, () => "Dereference requires type category:\n " + Dump());
            return Type.AutomaticDereferenceResult(CompleteCategory).ReplaceArg(this);
        }

        internal static Result ConcatPrintResult(Category category, int count, Func<int, Result> elemResults)
        {
            var result = TypeBase.VoidResult(category);
            if(!(category.HasCode || category.HasArgs))
                return result;

            if(category.HasCode)
                result.Code = CodeBase.DumpPrintText("(");

            for(var i = 0; i < count; i++)
            {
                var elemResult = elemResults(i);
                result.IsDirty = true;
                if(category.HasCode)
                {
                    if(i > 0)
                        result.Code = result.Code.Sequence(CodeBase.DumpPrintText(", "));
                    result.Code = result.Code.Sequence(elemResult.Code);
                }
                if(category.HasArgs)
                    result.CodeArgs = result.CodeArgs.Sequence(elemResult.CodeArgs);
                result.IsDirty = false;
            }
            if(category.HasCode)
                result.Code = result.Code.Sequence(CodeBase.DumpPrintText(")"));
            return result;
        }

        [DebuggerHidden]
        public static Result operator &(Result result, Category category) { return result.Filter(category); }

        [DebuggerHidden]
        public static Result operator |(Result aResult, Result bResult)
        {
            Tracer.Assert((aResult.CompleteCategory & bResult.CompleteCategory).IsNone);
            var result = aResult.Clone();
            result.Update(bResult);
            return result;
        }

        internal Result ConvertToBitSequence(Category category)
        {
            return Type
                .ConvertToBitSequence(category)
                .ReplaceArg(this);
        }

        internal Result LocalReferenceResult(RefAlignParam refAlignParam)
        {
            return Type
                .LocalReferenceResult(CompleteCategory, refAlignParam)
                .ReplaceArg(this);
        }

        internal Result ContextReferenceViaStructReference(Structure accessPoint)
        {
            return accessPoint
                .ContextReferenceViaStructReference(this);
        }

        [DebuggerHidden]
        internal Result AssertVoidOrValidReference()
        {
            var size = FindSize;
            if(size != null)
                Tracer.Assert(size.IsZero || size == Root.DefaultRefAlignParam.RefSize, Dump);

            if(HasType)
                Tracer.Assert(Type is Type.Void || Type is ReferenceType, Dump);
            return this;
        }

        [DebuggerHidden]
        internal Result AssertValidReference()
        {
            var size = FindSize;
            if(size != null)
                Tracer.Assert(size == Root.DefaultRefAlignParam.RefSize, Dump);

            if(HasType)
                Tracer.Assert(Type is ReferenceType, Dump);
            return this;
        }

        [DebuggerHidden]
        internal Result AssertEmptyOrValidReference()
        {
            var size = FindSize;
            if(size != null)
            {
                if(size.IsZero)
                    return this;
                Tracer.Assert(size == Root.DefaultRefAlignParam.RefSize, Dump);
            }

            if(HasType)
                Tracer.Assert(Type is ReferenceType, Dump);
            return this;
        }
    }

    internal sealed class Error
    {
        private readonly ContextBase _context;
        private readonly CompileSyntax _syntax;

        internal Error(ContextBase context, CompileSyntax syntax)
        {
            _context = context;
            _syntax = syntax;
        }

        private Error(Error e0, Error e1) { }

        public static Error operator +(Error e0, Error e1)
        {
            if(e0 == null)
                return e1;
            return e1 == null ? e0 : new Error(e0, e1);
        }

        internal ContextBase Context { get { return _context; } }
        internal CompileSyntax Syntax { get { return _syntax; } }
    }
}