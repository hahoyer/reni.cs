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
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Syntax;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Struct
{
    internal sealed class Structure : ReniObject
    {
        private readonly ContainerContextObject _containerContextObject;
        private readonly int _endPosition;
        private readonly DictionaryEx<ICompileSyntax, FunctionalBody> _functionalFeatureCache;
        private readonly SimpleCache<StructureType> _typeCache;
        private readonly DictionaryEx<int, AccessType> _accessTypesCache;

        internal Structure(ContainerContextObject containerContextObject, int endPosition)
        {
            _containerContextObject = containerContextObject;
            _endPosition = endPosition;
            _functionalFeatureCache = new DictionaryEx<ICompileSyntax, FunctionalBody>(body => new FunctionalBody(this, body));
            _typeCache = new SimpleCache<StructureType>(() => new StructureType(this));
            _accessTypesCache = new DictionaryEx<int, AccessType>(position => new AccessType(this, position));
        }

        [EnableDump]
        internal int EndPosition { get { return _endPosition; } }

        [EnableDump]
        internal ContainerContextObject ContainerContextObject { get { return _containerContextObject; } }

        [DisableDump]
        internal ContextBase UniqueContext
        {
            get
            {
                return ContainerContextObject
                    .Parent
                    .UniqueChildContext(ContainerContextObject.Container, EndPosition);
            }
        }

        [DisableDump]
        internal StructureType Type { get { return _typeCache.Value; } }

        [DisableDump]
        internal AutomaticReferenceType ReferenceType { get { return Type.UniqueAutomaticReference(RefAlignParam); } }

        internal override string DumpShort() { return base.DumpShort() + "(" + ContainerContextObject.DumpShort() + "@" + EndPosition + ")"; }

        [DisableDump]
        internal RefAlignParam RefAlignParam { get { return ContainerContextObject.RefAlignParam; } }

        [DisableDump]
        internal TypeBase IndexType { get { return ContainerContextObject.IndexType; } }

        private bool _isObtainStructSizeActive;

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
                    var result = ContainerContextObject.StructSize(EndPosition);
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

        private sealed class RecursionWhileObtainingStructSizeException : Exception
        {
            [EnableDump]
            private readonly Structure _structure;
            public RecursionWhileObtainingStructSizeException(Structure structure) { _structure = structure; }
        }

        [DisableDump]
        internal bool IsZeroSized { get { return ContainerContextObject.IsZeroSized(EndPosition); } }

        internal FunctionalBody UniqueFunctionalFeature(ICompileSyntax body) { return _functionalFeatureCache.Find(body); }
        internal AccessType UniqueAccessType(int position) { return _accessTypesCache.Find(position); }

        internal Result AccessViaThisReference(Category category, Result rightResult)
        {
            var position = rightResult
                .Conversion(IndexType)
                .Evaluate()
                .ToInt32();
            return AccessViaThisReference(category, position);
        }

        private ICompileSyntax[] Statements { get { return ContainerContextObject.Statements; } }


        internal Size FieldOffset(int position) { return ContainerContextObject.FieldOffsetFromAccessPoint(EndPosition, position); }

        internal Result DumpPrintResultViaStructReference(Category category)
        {
            return DumpPrintResultViaContextReference(category)
                .ContextReferenceViaStructReference(this);
        }

        internal Result DumpPrintResultViaContextReference(Category category) { return Result.ConcatPrintResult(category, EndPosition, position => DumpPrintResultViaAccessReference(category, position)); }

        internal Result AccessViaThisReference(Category category, int position)
        {
            return UniqueAccessType(position)
                .Result(category, () => ReferenceType.ArgCode());
        }

        internal ISearchPath<IFeature, StructureType> SearchFromRefToStruct(Defineable defineable)
        {
            return ContainerContextObject
                .Container
                .SearchFromRefToStruct(defineable);
        }

        internal Result CreateFunctionCall(Category category, ICompileSyntax body, Result argsResult)
        {
            return (ContainerContextObject
                .RootContext
                .CreateFunctionCall(this, category, body, argsResult));
        }

        internal Result AccessViaContextReference(Category category, int position)
        {
            var accessType = UniqueAccessType(position);
            return ContainerContextObject
                .AccessFromContextReference(category, accessType, EndPosition);
        }

        internal Result StructReferenceViaContextReference(Category category)
        {
            return ReferenceType
                .Result
                (category
                 , StructReferenceCodeViaContextReference
                 , () => Refs.Create(ContainerContextObject)
                );
        }

        private Result DumpPrintResultViaAccessReference(Category category, int position)
        {
            var accessType = UniqueAccessType(position);
            return accessType
                .DumpPrintOperationResult(category)
                .ReplaceArg(accessType.FieldReferenceViaStructReference(category | Category.Type));
        }

        internal Result ContextReferenceViaStructReference(Result result) { return ContainerContextObject.ContextReferenceViaStructReference(EndPosition, result); }
        private CodeBase StructReferenceCodeViaContextReference() { return CodeBase.ReferenceCode(ContainerContextObject).AddToReference(RefAlignParam, StructSize*-1); }
        internal TypeBase ValueType(int position) { return ContainerContextObject.InnerType(EndPosition, position).UnAlignedType; }
    }
}