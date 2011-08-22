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
        private Size _size;
        private TypeBase _type;
        private CodeBase _code;
        private Refs _refs;

        internal Result()
            : base(_nextObjectId++)
        {
            PendingCategory = new Category();
            StopByObjectId(-135);
        }

        internal Result(Category category, Func<Size> getSize, Func<TypeBase> getType, Func<CodeBase> getCode, Func<Refs> getRefs)
            : this()
        {
            if(category.HasSize)
                _size = getSize();
            if(category.HasType)
                _type = getType();
            if(category.HasCode)
                _code = getCode();
            if(category.HasRefs)
                _refs = getRefs();
            AssertValid();
        }

        internal Result(Category category, Func<Size> getSize, Func<CodeBase> getCode, Func<Refs> getRefs)
            : this()
        {
            if(category.HasSize)
                _size = getSize();
            if(category.HasCode)
                _code = getCode();
            if(category.HasRefs)
                _refs = getRefs();
            AssertValid();
        }

        internal Result(Category category, Func<Refs> getRefs)
            : this(category, () => Size.Zero, CodeBase.Void, getRefs) { }

        private bool HasSize { get { return Size != null; } }
        internal bool HasType { get { return Type != null; } }
        internal bool HasCode { get { return Code != null; } }
        internal bool HasRefs { get { return Refs != null; } }

        [Node]
        [EnableDumpWithExceptions]
        internal Category PendingCategory;

        public Category CompleteCategory { get { return new Category(HasSize, HasType, HasCode, HasRefs); } }

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
        public Refs Refs
        {
            get { return _refs; }
            set
            {
                _refs = value;
                AssertValid();
            }
        }

        TreeNode[] ITreeNodeSupport.CreateNodes()
        {
            var result = new List<TreeNode>();
            if(PendingCategory.HasAny)
                result.Add(Dump().CreateNamedNode("Pending", "Pending"));
            if(HasSize)
                result.Add(Size.FormatForView().CreateNamedNode("Size", "Number"));
            if(HasType)
                result.Add(Type.CreateNamedNode("Type", "Type"));
            if(HasCode)
                result.Add(Code.CreateNamedNode("Code", "Code"));
            if(HasRefs)
                result.Add(Refs.Data.CreateNamedNode("Refs", "Refs"));
            return result.ToArray();
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

        internal Refs FindRefs
        {
            get
            {
                if(HasRefs)
                    return Refs;
                if(HasCode)
                    return Code.Refs;
                return null;
            }
        }

        [DisableDump]
        internal Refs SmartRefs
        {
            get
            {
                var result = FindRefs;
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
                if(CompleteCategory.HasSize && !Size.IsZero)
                    return false;
                if(CompleteCategory.HasType && !(Type is Type.Void))
                    return false;
                if(CompleteCategory.HasCode && !Code.IsEmpty)
                    return false;
                if(CompleteCategory.HasRefs && !Refs.IsNone)
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
                if(CompleteCategory.HasRefs && !Refs.IsNone)
                    return false;
                return true;
            }
        }

        internal bool HasArg
        {
            get
            {
                if(HasRefs)
                    return Refs.HasArg;
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
            if(HasSize)
                result += "\nSize=" + Tracer.Dump(_size);
            if(HasType)
                result += "\nType=" + Tracer.Dump(_type);
            if(HasRefs)
                result += "\nRefs=" + Tracer.Dump(_refs);
            if(HasCode)
                result += "\nCode=" + Tracer.Dump(_code);
            return result;
        }

        internal void Update(Result result)
        {
            if(result.HasSize)
                _size = result.Size;

            if(result.HasType)
                _type = result.Type;

            if(result.HasRefs)
                _refs = result.Refs;

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

            if(category.HasSize)
                result._size = Size;
            if(category.HasType)
                result._type = Type;
            if(category.HasRefs)
                result._refs = Refs;
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

            var r = new Result();
            if(HasSize)
                r.Size = alignedSize;
            if(HasType)
                r.Type = Type.UniqueAlign(alignBits);
            if(HasCode)
                r.Code = Code.BitCast(alignedSize);
            if(HasRefs)
                r.Refs = Refs;
            return r;
        }

        private Result Clone(Category category)
        {
            var r = new Result {PendingCategory = PendingCategory & category};
            if(category.HasSize)
                r.Size = Size;
            if(category.HasType)
                r.Type = Type;
            if(category.HasCode)
                r.Code = Code;
            if(category.HasRefs)
                r.Refs = Refs;
            return r;
        }

        internal Result Clone() { return new Result {PendingCategory = PendingCategory, Size = Size, Type = Type, Code = Code, Refs = Refs}; }

        private void AssertValid()
        {
            if(IsDirty)
                return;

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

            if(HasRefs && HasCode)
            {
                var refs = Refs;
                var codeRefs = Code.Refs;
                if(!(refs.Contains(codeRefs) && codeRefs.Contains(refs)))
                    Tracer.AssertionFailed(1, @"Refs.Contains(codeRefs)", () => "Code and Refs differ " + Dump());
            }
        }

        internal void AssertComplete(Category category, CompileSyntax syntaxForDump)
        {
            Tracer.Assert
                (
                    1,
                    category <= (CompleteCategory | PendingCategory),
                    () => string.Format("syntax={2}\ncategory={0}\nResult={1}", category, Dump(), syntaxForDump.DumpShort())
                );
        }

        private void Add(Result other) { Add(other, CompleteCategory); }

        private void Add(Result other, Category category)
        {
            Tracer.Assert(category <= other.CompleteCategory);
            Tracer.Assert(category <= CompleteCategory);
            IsDirty = true;
            if(category.HasSize)
                Size += other.Size;
            if(category.HasType)
                Type = Type.Pair(other.Type);
            if(category.HasCode)
                Code = Code.Sequence(other.Code);
            if(category.HasRefs)
                Refs = Refs.Sequence(other.Refs);
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
            var result = new Result {Size = Size, Type = Type, IsDirty = true};
            if(HasCode && resultForArg.HasCode)
                result.Code = Code.ReplaceArg(resultForArg.Type, resultForArg.Code);
            if(HasRefs && resultForArg.HasRefs)
                result.Refs = Refs.WithoutArg().Sequence(resultForArg.Refs);
            result.IsDirty = false;
            return result;
        }

        internal Result ReplaceAbsolute<TRefInCode>(TRefInCode refInCode, Func<CodeBase> replacementCode, Func<Refs> replacementRefs)
            where TRefInCode : IReferenceInCode
        {
            if(HasRefs && !Refs.Contains(refInCode))
                return this;

            var result = new Result {Size = Size, Type = Type, IsDirty = true};
            if(HasCode)
                result.Code = Code.ReplaceAbsolute(refInCode, replacementCode);
            if(HasRefs)
                result.Refs = Refs.Without(refInCode).Sequence(replacementRefs());
            result.IsDirty = false;
            return result;
        }

        internal Result ReplaceAbsolute<TRefInCode>(TRefInCode refInCode, Func<Category, Result> getReplacement)
            where TRefInCode : IReferenceInCode
        {
            if(HasRefs && !Refs.Contains(refInCode))
                return this;

            var replacement = getReplacement(CompleteCategory - Category.Size - Category.Type);
            var result = new Result {Size = Size, Type = Type, IsDirty = true};
            if(HasCode)
                result.Code = Code.ReplaceAbsolute(refInCode, () => replacement.Code);
            if(HasRefs)
                result.Refs = Refs.Without(refInCode).Sequence(replacement.Refs);
            result.IsDirty = false;
            return result;
        }

        internal Result ReplaceRelative<TRefInCode>(TRefInCode refInCode, Func<CodeBase> replacementCode, Func<Refs> replacementRefs)
            where TRefInCode : IReferenceInCode
        {
            if(HasRefs && !Refs.Contains(refInCode))
                return this;

            var result = new Result {Size = Size, Type = Type};
            if(HasCode)
                result.Code = Code.ReplaceRelative(refInCode, replacementCode);
            if(HasRefs)
                result.Refs = Refs.Without(refInCode).Sequence(replacementRefs());
            return result;
        }

        internal Result ReplaceRefsForFunctionBody(RefAlignParam refAlignParam, CodeBase replacement)
        {
            if(!HasCode)
                return this;
            if(SmartRefs.Count == 0)
                return this;
            var result = Clone();
            result.IsDirty = true;
            result.Code = SmartRefs.ReplaceRefsForFunctionBody(Code, refAlignParam, replacement);
            result.Refs = replacement.Refs;
            result.IsDirty = false;
            return result;
        }

        internal Result LocalBlock(Category category)
        {
            if(!category.HasCode && !category.HasRefs)
                return this;

            var result = Clone(category);
            var copier = Type.Copier(category);
            if(category.HasCode)
                result.Code = Code.LocalBlock(copier.Code);
            if(category.HasRefs)
                result.Refs = Refs.Sequence(copier.Refs);
            return result;
        }

        internal Result Conversion(TypeBase target) { return Type.Conversion(CompleteCategory, target).ReplaceArg(this); }

        internal BitsConst Evaluate()
        {
            Tracer.Assert(Refs.IsNone);
            var result = Align(3).LocalBlock(CompleteCategory);
            return result.Code.Evaluate();
        }

        internal Result AutomaticDereference()
        {
            if(CompleteCategory == Category.Refs)
                return this;

            Tracer.Assert(HasType, () => "Dereference requires type category:\n " + Dump());
            return Type.AutomaticDereferenceResult(CompleteCategory).ReplaceArg(this);
        }

        internal static Result ConcatPrintResult(Category category, int count, Func<int, Result> elemResults)
        {
            var result = TypeBase.VoidResult(category);
            if(!(category.HasCode || category.HasRefs))
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
                if(category.HasRefs)
                    result.Refs = result.Refs.Sequence(elemResult.Refs);
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