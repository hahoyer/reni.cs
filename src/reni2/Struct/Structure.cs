// 
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

using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using HWClassLibrary.TreeStructure;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Syntax;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Struct
{
    sealed class Structure : ReniObject
    {
        static int _nextObjectId;
        readonly ContainerContextObject _containerContextObject;
        readonly int _endPosition;
        [Node]
        readonly SimpleCache<StructureType> _typeCache;
        [Node]
        readonly DictionaryEx<int, TypeBase> _accessTypesCache;
        [Node]
        readonly DictionaryEx<int, AccessFeature> _accessFeaturesCache;
        [Node]
        readonly DictionaryEx<int, ContextAccessFeature> _contextAccessFeaturesCache;

        internal Structure(ContainerContextObject containerContextObject, int endPosition)
            : base(_nextObjectId++)
        {
            _containerContextObject = containerContextObject;
            _endPosition = endPosition;
            _typeCache = new SimpleCache<StructureType>(() => new StructureType(this));
            _accessTypesCache = new DictionaryEx<int, TypeBase>(ObtainAccessType);
            _accessFeaturesCache = new DictionaryEx<int, AccessFeature>(position => new AccessFeature(this, position));
            _contextAccessFeaturesCache = new DictionaryEx<int, ContextAccessFeature>(position => new ContextAccessFeature(this, position));
            StopByObjectId(-313);
        }

        [EnableDump]
        [Node]
        internal int EndPosition { get { return _endPosition; } }

        [EnableDump]
        [Node]
        internal ContainerContextObject ContainerContextObject { get { return _containerContextObject; } }

        [DisableDump]
        internal ContextBase UniqueContext
        {
            get
            {
                return ContainerContextObject
                    .Parent
                    .UniqueStructurePositionContext(ContainerContextObject.Container, EndPosition);
            }
        }

        [DisableDump]
        internal StructureType Type { get { return _typeCache.Value; } }

        [DisableDump]
        internal TypeBase ReferenceType { get { return Type.SmartReference(RefAlignParam); } }

        internal override string DumpShort() { return base.DumpShort() + "(" + ContainerContextObject.DumpShort() + "@" + EndPosition + ")"; }

        [DisableDump]
        internal RefAlignParam RefAlignParam { get { return ContainerContextObject.RefAlignParam; } }

        [DisableDump]
        internal TypeBase IndexType { get { return ContainerContextObject.IndexType; } }

        bool _isObtainStructSizeActive;

        [DisableDump]
        internal Size StructSize
        {
            get
            {
                if(_isObtainStructSizeActive)
                    throw new RecursionWhileObtainingStructSizeException(this);

                try
                {
                    _isObtainStructSizeActive = true;
                    var result = ContainerContextObject.StructureSize(EndPosition);
                    _isObtainStructSizeActive = false;
                    return result;
                }
                catch(RecursionWhileObtainingStructSizeException)
                {
                    _isObtainStructSizeActive = false;
                    return null;
                }
            }
        }

        [DisableDump]
// ReSharper disable PossibleInvalidOperationException
            internal bool IsDataLess
        {
            get { return StructIsDataLess(false).Value; }
        }
// ReSharper restore PossibleInvalidOperationException

        internal bool? StructIsDataLess(bool isQuick) { return ContainerContextObject.StructureIsDataLess(isQuick, EndPosition); }

        sealed class RecursionWhileObtainingStructSizeException : Exception
        {
            [EnableDump]
            readonly Structure _structure;
            public RecursionWhileObtainingStructSizeException(Structure structure) { _structure = structure; }
        }

        internal TypeBase FunctionalFeature(CompileSyntax getter, CompileSyntax setter, bool isAutoCall) { return new FunctionalBodyType(this, getter, setter, isAutoCall); }
        TypeBase UniqueAccessType(int position) { return _accessTypesCache.Find(position); }
        internal AccessFeature UniqueAccessFeature(int position) { return _accessFeaturesCache.Find(position); }
        internal IContextFeature UniqueContextAccessFeature(int position) { return _contextAccessFeaturesCache.Find(position); }

        TypeBase ObtainAccessType(int position)
        {
            var accessType = ContainerContextObject.AccessType(EndPosition, position);
            if(accessType.IsLambda)
                return accessType;
            if(!accessType.IsDataLess)
                return accessType.UniqueFieldAccessType(RefAlignParam, FieldOffset(position));

            NotImplementedMethod(position);
            return null;
        }

        internal Result AccessViaThisReference(Category category, Result rightResult)
        {
            var position = rightResult
                .Conversion(IndexType)
                .Evaluate(ContainerContextObject.RootContext.OutStream)
                .ToInt32();
            return AccessViaThisReference(category, position);
        }

        internal Size FieldOffset(int position) { return ContainerContextObject.FieldOffsetFromAccessPoint(EndPosition, position); }

        internal Result DumpPrintResultViaStructReference(Category category)
        {
            return DumpPrintResultViaContextReference(category)
                .ContextReferenceViaStructReference(this);
        }

        internal Result DumpPrintResultViaContextReference(Category category) { return Result.ConcatPrintResult(category, EndPosition, position => DumpPrintResultViaAccessReference(category, position)); }

        internal Result AccessViaThisReference(Category category, int position)
        {
            var resultType = UniqueAccessType(position);
            if(resultType.IsDataLess)
                return resultType.Result(category);

            return resultType
                .Result(category, ReferenceType.ArgResult(category));
        }

        internal ISearchPath<ISuffixFeature, StructureType> SearchFromRefToStruct(Defineable defineable)
        {
            return ContainerContextObject
                .Container
                .SearchFromRefToStruct(defineable);
        }

        internal Result Call(Category category, CompileSyntax body, Result argsResult)
        {
            return ContainerContextObject
                .RootContext
                .Call(this, category, body, argsResult);
        }

        internal bool IsObjectForCallRequired(CompileSyntax body) { return true; }

        internal Result AccessViaContextReference(Category category, int position)
        {
            var accessType = UniqueAccessType(position);
            var result = accessType.Result(category, ContainerContextObject, ContextOffset);
            return result;
        }
        Size ContextOffset() { return ContainerContextObject.ContextReferenceOffsetFromAccessPoint(EndPosition) * -1; }

        internal Result StructReferenceViaContextReference(Category category)
        {
            if(IsDataLess)
                return Type.Result(category);

            return ReferenceType
                .Result
                (category
                 , StructReferenceCodeViaContextReference
                 , () => CodeArgs.Create(ContainerContextObject)
                );
        }

        Result DumpPrintResultViaAccessReference(Category category, int position)
        {
            var accessType = UniqueAccessType(position);
            return accessType
                .GenericDumpPrintResult(category,RefAlignParam)
                .ReplaceArg(AccessViaThisReference(category.Typed,position));
        }

        internal Result ContextReferenceViaStructReference(Result result)
        {
            return ContainerContextObject
                .ContextReferenceViaStructReference(EndPosition, result);
        }
        CodeBase StructReferenceCodeViaContextReference()
        {
            return CodeBase.ReferenceCode(ContainerContextObject)
                .AddToReference(RefAlignParam, StructSize * -1);
        }

        internal TypeBase ValueType(int position)
        {
            return ContainerContextObject
                .AccessType(EndPosition, position)
                .UnAlignedType;
        }

        internal IStructFeature SearchFromStructContext(Defineable defineable) { return ContainerContextObject.Container.SearchFromStructContext(defineable); }
    }
}