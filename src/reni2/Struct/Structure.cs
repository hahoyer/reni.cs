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

        internal Structure(ContainerContextObject containerContextObject, int endPosition)
        {
            _containerContextObject = containerContextObject;
            _endPosition = endPosition;
            _functionalFeatureCache = new DictionaryEx<ICompileSyntax, FunctionalBody>(body => new FunctionalBody(this, body));
            _typeCache = new SimpleCache<StructureType>(() => new StructureType(this));
        }

        [EnableDump]
        internal int EndPosition { get { return _endPosition; } }

        [EnableDump]
        internal ContainerContextObject ContainerContextObject { get { return _containerContextObject; } }

        [DisableDump]
        internal ContextBase SpawnContext
        {
            get
            {
                return ContainerContextObject
                    .Parent
                    .SpawnChildContext(ContainerContextObject.Container, EndPosition);
            }
        }

        [DisableDump]
        internal StructureType Type { get { return _typeCache.Value; } }

        [DisableDump]
        internal AutomaticReferenceType StructureReferenceType { get { return Type.SpawnReference(RefAlignParam); } }

        [DisableDump]
        internal Refs ConstructorRefs
        {
            get
            {
                return ContainerContextObject
                    .Container
                    .InnerResult(Category.Refs, ContainerContextObject.Parent, EndPosition).Refs;
            }
        }

        internal override string DumpShort() { return base.DumpShort() + "(" + ContainerContextObject.DumpShort() + "@" + EndPosition + ")"; }

        [DisableDump]
        internal RefAlignParam RefAlignParam { get { return ContainerContextObject.RefAlignParam; } }

        [DisableDump]
        internal TypeBase IndexType { get { return ContainerContextObject.IndexType; } }

        [DisableDump]
        internal Size StructSize { get { return ContainerContextObject.StructSize(EndPosition); } }

        internal FunctionalBody SpawnFunctionalFeature(ICompileSyntax body) { return _functionalFeatureCache.Find(body); }

        internal Result AccessViaThisReference(Category category, Result rightResult)
        {
            var position = rightResult
                .ConvertTo(IndexType)
                .Evaluate()
                .ToInt32();
            return AccessViaThisReference(category, position);
        }

        private ICompileSyntax[] Statements { get { return ContainerContextObject.Statements; } }

        internal Result AccessViaThisReference(Category category, int position)
        {
            return AccessType(position)
                .Result(category, ()=> StructureReferenceType.ArgCode());
        }
        internal Result ReplaceContextReferenceByThisReference(Category category) { return ReplaceContextReferenceByThisReference(DumpPrintResultFromContextReference(category)); }

        internal ISearchPath<IFeature, StructureType> SearchFromRefToStruct(Defineable defineable)
        {
            return ContainerContextObject
                .Container
                .SearchFromRefToStruct(defineable);
        }

        internal Result CreateFunctionCall(Category category, ICompileSyntax body, Result argsResult)
        {
            return ContainerContextObject
                .RootContext
                .CreateFunctionCall(this, category, body, argsResult);
        }

        internal AccessType AccessType(int position)
        {
            return ContainerContextObject
                .InnerType(position)
                .AccessType(this, position);
        }

        internal Result DumpPrintResultFromContextReference(Category category)
        {
            var result = Result.ConcatPrintResult(category, EndPosition, position => DumpPrintResultFromThisReference(category, position));
            return result;
        }

        internal Result AccessViaContextReference(Category category, int position)
        {
            var accessType = AccessType(position);
            return ContainerContextObject
                .AccessFromContextReference(category, accessType, EndPosition);
        }

        internal Result ThisReferenceResultViaContextReference(Category category)
        {
            var result = Type.SpawnReference(RefAlignParam).Result
                (category
                 , ThisReferenceViaContextReferenceCode
                 , () => Refs.Create(ContainerContextObject)
                );
            return result;
        }

        private Result DumpPrintResultFromThisReference(Category category, int position)
        {
            return ContainerContextObject
                .InnerType(position)
                .SpawnAccessType(this, position)
                .DumpPrintOperationResult(category)
                .ReplaceArg(AccessViaThisReference(category, position));
        }

        private Result ReplaceContextReferenceByThisReference(Result result) { return ContainerContextObject.ReplaceContextReferenceByThisReference(EndPosition, result); }
        private CodeBase ThisReferenceViaContextReferenceCode() { return CodeBase.ReferenceCode(ContainerContextObject).AddToReference(RefAlignParam, StructSize*-1); }

        internal Size FieldOffsetFromThisReference(int position) { return ContainerContextObject.FieldOffsetFromAccessPoint(EndPosition, position); }
    }
}