#region Copyright (C) 2012

//     Project Reni2
//     Copyright (C) 2011 - 2012 Harald Hoyer
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

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using HWClassLibrary.TreeStructure;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Struct;
using Reni.Type;

namespace Reni
{
    [Serializable]
    sealed class Result : ReniObject, ITreeNodeSupport
    {
        static int _nextObjectId = 1;
        bool _isDirty;
        bool? _isDataLess;
        Size _size;
        TypeBase _type;
        CodeBase _code;
        CodeArgs _codeArgs;
        Category _pendingCategory;

        internal Result()
            : base(_nextObjectId++)
        {
            _pendingCategory = new Category();
            StopByObjectId(-804);
        }

        internal Result(Category category, Func<bool> getDataLess = null, Func<Size> getSize = null, Func<TypeBase> getType = null, Func<CodeBase> getCode = null, Func<CodeArgs> getArgs = null)
            : this()
        {
            var isDataLess = getDataLess == null ? null : new SimpleCache<bool>(getDataLess);
            var size = getSize == null ? null : new SimpleCache<Size>(getSize);
            var type = getType == null ? null : new SimpleCache<TypeBase>(getType);
            var code = getCode == null ? null : new SimpleCache<CodeBase>(getCode);
            var codeArgs = getArgs == null ? null : new SimpleCache<CodeArgs>(getArgs);

            if(category.HasType)
                _type = ObtainType(isDataLess, size, type, code);

            if(category.HasCode)
                _code = ObtainCode(isDataLess, size, type, code);

            if(category.HasSize)
                _size = ObtainSize(isDataLess, size, type, code);

            if(category.HasArgs)
                _codeArgs = ObtainCodeArgs(isDataLess, size, type, code, codeArgs);

            if(category.HasIsDataLess)
                _isDataLess = ObtainIsDataLess(isDataLess, size, type, code);

            AssertValid();
        }

        CodeBase ObtainCode(SimpleCache<bool> getDataLess, SimpleCache<Size> getSize, SimpleCache<TypeBase> getType, SimpleCache<CodeBase> getCode)
        {
            if(getCode != null)
                return getCode.Value;
// ReSharper disable ExpressionIsAlwaysNull
            var isDataLess = TryObtainIsDataLess(getDataLess, getSize, getType, getCode);
// ReSharper restore ExpressionIsAlwaysNull
            if(isDataLess == true)
                return CodeBase.Void;
            Tracer.AssertionFailed("Code cannot be determned", ToString);
            return null;
        }

        TypeBase ObtainType(SimpleCache<bool> getDataLess, SimpleCache<Size> getSize, SimpleCache<TypeBase> getType, SimpleCache<CodeBase> getCode)
        {
            if(getType != null)
                return getType.Value;
// ReSharper disable ExpressionIsAlwaysNull
            var isDataLess = TryObtainIsDataLess(getDataLess, getSize, getType, getCode);
// ReSharper restore ExpressionIsAlwaysNull
            if(isDataLess == true)
                return TypeBase.Void;
            Tracer.AssertionFailed("Type cannot be determned", ToString);
            return null;
        }

        Size ObtainSize(SimpleCache<bool> getDataLess, SimpleCache<Size> getSize, SimpleCache<TypeBase> getType, SimpleCache<CodeBase> getCode)
        {
            var result = TryObtainSize(getDataLess, getSize, getType, getCode);
            Tracer.Assert(result != null, () => "Size cannot be determned " + ToString());
            return result;
        }

        static Size TryObtainSize(SimpleCache<bool> getDataLess, SimpleCache<Size> getSize, SimpleCache<TypeBase> getType, SimpleCache<CodeBase> getCode)
        {
            if(getSize != null)
                return getSize.Value;
            if(getType != null)
                return getType.Value.Size;
            if(getCode != null)
                return getCode.Value.Size;
            if(getDataLess != null && getDataLess.Value)
                return Size.Zero;
            return null;
        }

        bool ObtainIsDataLess(SimpleCache<bool> getDataLess, SimpleCache<Size> getSize, SimpleCache<TypeBase> getType, SimpleCache<CodeBase> getCode)
        {
            var result = TryObtainIsDataLess(getDataLess, getSize, getType, getCode);
            if(result != null)
                return result.Value;
            Tracer.AssertionFailed("Datalessness cannot be determned", ToString);
            return false;
        }

        static bool? TryObtainIsDataLess(SimpleCache<bool> getDataLess, SimpleCache<Size> getSize, SimpleCache<TypeBase> getType, SimpleCache<CodeBase> getCode)
        {
            if(getDataLess != null)
                return getDataLess.Value;
            if(getSize != null)
                return getSize.Value.IsZero;
            if(getType != null)
                return getType.Value.IsDataLess;
            if(getCode != null)
                return getCode.Value.IsEmpty;
            return null;
        }

        CodeArgs ObtainCodeArgs(SimpleCache<bool> getDataLess, SimpleCache<Size> getSize, SimpleCache<TypeBase> getType, SimpleCache<CodeBase> getCode, SimpleCache<CodeArgs> getArgs)
        {
            if(getArgs != null)
                return getArgs.Value;
            if(getCode != null)
                return getCode.Value.CodeArgs;
// ReSharper disable ExpressionIsAlwaysNull
            if(TryObtainIsDataLess(getDataLess, getSize, getType, getCode) == true)
// ReSharper restore ExpressionIsAlwaysNull
                return CodeArgs.Void();

            Tracer.AssertionFailed("CodeArgs cannot be determned", ToString);
            return null;
        }

        internal bool HasSize { get { return Size != null; } }
        internal bool HasType { get { return Type != null; } }
        internal bool HasCode { get { return Code != null; } }
        internal bool HasArgs { get { return CodeArgs != null; } }
        internal bool HasIsDataLess { get { return _isDataLess != null; } }

        [Node]
        [EnableDumpWithExceptionPredicate]
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
            if(_pendingCategory.HasAny)
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
                result.Add(CodeArgs.Data.CreateNamedNode("CodeArgs", "CodeArgs"));
            return result.ToArray();
        }

        internal bool? FindIsDataLess
        {
            get
            {
                if(HasIsDataLess)
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

        bool IsDirty
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
        public Category PendingCategory
        {
            get { return _pendingCategory; }
            set
            {
                _pendingCategory = value;
                AssertValid();
            }
        }


        public override string DumpData()
        {
            var result = "";
            if(_pendingCategory != Category.None)
                result += "\nPendingCategory=" + _pendingCategory.Dump();
            if(CompleteCategory != Category.None)
                result += "\nCompleteCategory=" + CompleteCategory.Dump();
            if(HasIsDataLess)
                result += "\nIsDataLess=" + Tracer.Dump(_isDataLess);
            if(HasSize)
                result += "\nSize=" + Tracer.Dump(_size);
            if(HasType)
                result += "\nType=" + Tracer.Dump(_type);
            if(HasArgs)
                result += "\nCodeArgs=" + Tracer.Dump(_codeArgs);
            if(HasCode)
                result += "\nCode=" + Tracer.Dump(_code);
            if(result == "")
                return "";
            return result.Substring(1);
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

            _pendingCategory = _pendingCategory - result.CompleteCategory;

            AssertValid();
        }

        Result Filter(Category category)
        {
            return new Result
                (CompleteCategory & category
// ReSharper disable PossibleInvalidOperationException
                 , () => IsDataLess.Value
// ReSharper restore PossibleInvalidOperationException
                 , () => Size
                 , () => Type
                 , () => Code
                 , () => CodeArgs
                )
                   {_pendingCategory = _pendingCategory & category};
        }

        internal Result Align(int alignBits)
        {
            var size = FindSize;
            if(size == null)
                return this;

            var alignedSize = size.Align(alignBits);
            if(alignedSize == size)
                return this;

            var result = new Result
                (CompleteCategory
// ReSharper disable PossibleInvalidOperationException
                 , () => IsDataLess.Value
// ReSharper restore PossibleInvalidOperationException
                 , () => alignedSize
                 , () => Type.UniqueAlign(alignBits)
                 , () => Code.BitCast(alignedSize)
                 , () => CodeArgs
                );
            return result;
        }

        internal Result Clone() { return Filter(CompleteCategory); }

        void AssertValid()
        {
            if(IsDirty)
                return;

            var isDataLess = FindIsDataLess;
            if(isDataLess != null)
            {
                if(HasIsDataLess && IsDataLess != isDataLess.Value)
                    Tracer.AssertionFailed(@"IsDataLess==isDataLess", () => "IsDataLess differs " + Dump());
                if(HasSize && Size.IsZero != isDataLess.Value)
                    Tracer.AssertionFailed(@"Size.IsZero==isDataLess.Value", () => "Size differs " + Dump());
                if(HasType && Type.IsDataLess != isDataLess.Value)
                    Tracer.AssertionFailed(@"Type.IsDataLess==isDataLess.Value", () => "Type IsDataLess property differs " + Dump());
                if(HasCode && Code.Size.IsZero != isDataLess.Value)
                    Tracer.AssertionFailed(@"Code.Size.IsZero==isDataLess.Value", () => "Code size differs " + Dump());
            }

            var size = FindSize;
            if(size != null)
            {
                if(HasSize && Size != size)
                    Tracer.AssertionFailed(@"Size==size", () => "Size differs " + Dump());
                if(HasType && Type.Size != size)
                    Tracer.AssertionFailed(@"Type.Size==size", () => "Type size differs " + Dump());
                if(HasCode && Code.Size != size)
                    Tracer.AssertionFailed(@"Code.Size==size", () => "Code size differs " + Dump());
            }

            if(HasArgs && HasCode)
            {
                var refs = CodeArgs;
                var codeRefs = Code.CodeArgs;
                if(!(refs.Contains(codeRefs) && codeRefs.Contains(refs)))
                    Tracer.AssertionFailed(@"CodeArgs.Contains(codeRefs)", () => "Code and CodeArgs differ " + Dump());
            }

            Tracer.Assert((CompleteCategory & PendingCategory) == Category.None);
        }

        void Add(Result other) { Add(other, CompleteCategory); }

        void Add(Result other, Category category)
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

        Result Sequence(Result second)
        {
            var result = Clone();
            result.Add(second);
            return result;
        }

        internal Result ReplaceArg(Func<Category, Result> getResultForArg)
        {
            if(HasArg && getResultForArg != null)
                return InternalReplaceArg(getResultForArg);
            return this;
        }

        internal Result ReplaceArg(Result resultForArg)
        {
            if(HasArg && resultForArg != null)
                return InternalReplaceArg(category => resultForArg);
            return this;
        }

        Result InternalReplaceArg(Func<Category, Result> getResultForArg)
        {
            var result = new Result {IsDataLess = IsDataLess, Size = Size, Type = Type, Code = Code, CodeArgs = CodeArgs, IsDirty = true};

            var categoryForArg = (CompleteCategory & Category.Code.CodeArgsed);
            if (HasCode)
                categoryForArg |= Category.Type;

            var resultForArg = getResultForArg(categoryForArg);
            if(resultForArg != null)
            {
                if (HasCode)
                    result.Code = Code.ReplaceArg(getResultForArg(Category.Type | Category.Code));
                if (HasArgs)
                    result.CodeArgs = CodeArgs.WithoutArg() + getResultForArg(Category.CodeArgs).CodeArgs;
            }
            result.IsDirty = false;
            return result;
        }

        internal Result ReplaceAbsolute<TRefInCode>(TRefInCode refInCode, Func<CodeBase> replacementCode, Func<CodeArgs> replacementRefs)
            where TRefInCode : IContextReference
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
            where TRefInCode : IContextReference
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
            where TRefInCode : IContextReference
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

        internal Result ReplaceRefsForFunctionBody(Size refSize, CodeBase replacement)
        {
            if(!HasCode)
                return this;
            if(SmartArgs.Count == 0)
                return this;
            var result = Clone();
            result.IsDirty = true;
            result.Code = SmartArgs.ReplaceRefsForFunctionBody(Code, refSize, replacement);
            result.CodeArgs = replacement.CodeArgs;
            result.IsDirty = false;
            return result;
        }

        internal Result LocalBlock(Category category)
        {
            if(!category.HasCode && !category.HasArgs)
                return this;

            var result = this & category;
            var copier = Type.Copier(category);
            if(category.HasCode)
                result.Code = Code.LocalBlock(copier.Code);
            if(category.HasArgs)
                result.CodeArgs = CodeArgs.Sequence(copier.CodeArgs);
            return result;
        }

        internal Result Conversion(TypeBase target) { return Type.Conversion(CompleteCategory, target).ReplaceArg(this); }

        internal BitsConst Evaluate(IOutStream outStream)
        {
            Tracer.Assert(CodeArgs.IsNone);
            var result = Align(3).LocalBlock(CompleteCategory);
            return result.Code.Evaluate(outStream);
        }

        internal Result AutomaticDereferenceResult()
        {
            Tracer.Assert(HasType, () => "Dereference requires type category:\n " + Dump());

            var result = this;
            while(result.Type.Reference != null)
                result = result.DereferenceResult();
            return result;
        }

        internal Result DereferenceResult()
        {
            Tracer.Assert(HasType, () => "Dereference requires type category:\n " + Dump());
            return Type
                .Reference
                .DereferenceResult(CompleteCategory)
                .ReplaceArg(this);
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

        [DebuggerHidden]
        public static Result operator +(Result aResult, Result bResult) { return aResult.Sequence(bResult); }

        internal Result ConvertToBitSequence(Category category)
        {
            return Type
                .ConvertToBitSequence(category)
                .ReplaceArg(this);
        }

        internal Result SmartLocalReferenceResult()
        {
            if(Type.IsDataLess)
                return this;
            if(Type is IReference)
                return this;
            return Type
                .SmartLocalReferenceResult(CompleteCategory)
                .ReplaceArg(this);
        }

        internal Result ContextReferenceViaStructReference(Structure accessPoint)
        {
            return accessPoint
                .ContextReferenceViaStructReference(this);
        }

        [DebuggerHidden]
        internal void AssertVoidOrValidReference()
        {
            var size = FindSize;
            if(size != null)
                Tracer.Assert(size.IsZero || size == Root.DefaultRefAlignParam.RefSize, () => "Expected size: 0 or RefSize\n" + Dump());

            if(HasType)
                Tracer.Assert(Type is Type.Void || Type is AutomaticReferenceType, () => "Expected type: Void or AutomaticReferenceType\n" + Dump());
        }

        [DebuggerHidden]
        internal void AssertValidReference()
        {
            var size = FindSize;
            if(size != null)
                Tracer.Assert(size == Root.DefaultRefAlignParam.RefSize, () => "Expected size: RefSize\n" + Dump());

            if(HasType)
                Tracer.Assert(Type is AutomaticReferenceType, () => "Expected type: AutomaticReferenceType\n" + Dump());
        }

        [DebuggerHidden]
        internal void AssertEmptyOrValidReference()
        {
            if(FindIsDataLess == true)
                return;

            var size = FindSize;
            if(size != null)
            {
                if(size.IsZero)
                    return;
                Tracer.Assert(size == Root.DefaultRefAlignParam.RefSize, () => "Expected size: 0 or RefSize\n" + Dump());
            }

            if(HasType)
                Tracer.Assert(Type is AutomaticReferenceType, () => "Expected type: AutomaticReferenceType\n" + Dump());
        }

        internal Result AddToReference(Func<Size> func) { return Change(code => code.AddToReference(func())); }

        public Result Change(Func<CodeBase, CodeBase> func)
        {
            if(!HasCode)
                return this;
            var result = Clone();
            result.Code = func(Code);
            return result;
        }
    }
}