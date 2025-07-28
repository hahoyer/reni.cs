using hw.Scanner;
using Reni.Basics;
using Reni.Context;
using Reni.Feature;
using Reni.Validation;

namespace Reni.Type;

abstract partial class TypeBase
{
    sealed class CacheContainer
    {
        [Node]
        [SmartNode]
        public readonly FunctionCache<int, AlignType> Aligner;

        [Node]
        [SmartNode]
        public readonly FunctionCache<int, FunctionCache<string, ArrayType>> Array;

        [Node]
        [SmartNode]
        public readonly ValueCache<EnableCut> EnableCut;

        [Node]
        [SmartNode]
        public readonly ValueCache<IReference> ForcedReference;

        [Node]
        [SmartNode]
        public readonly ValueCache<FunctionInstanceType> FunctionInstanceType;

        [Node]
        [SmartNode]
        public readonly FunctionCache<TypeBase, ResultCache> Mutation;

        [Node]
        [SmartNode]
        public readonly FunctionCache<TypeBase, Pair> Pair;

        [Node]
        [SmartNode]
        public readonly ValueCache<PointerType> Pointer;

        public readonly ValueCache<Size> Size;

        [Node]
        [SmartNode]
        public readonly ValueCache<IConversion[]> SymmetricConversions;

        [Node]
        [SmartNode]
        public readonly ValueCache<TypeType> TypeType;

        [Node]
        [SmartNode]
        internal readonly FunctionCache<string, ArrayReferenceType> ArrayReferenceCache;

        [Node]
        [SmartNode]
        public readonly FunctionCache<SourcePart, Issue> MissingDeclarationIssue;

        
        public CacheContainer(TypeBase parent)
        {
            EnableCut = new(() => new(parent));
            Mutation = new(destination =>
                ResultCache.CreateInstance(category => parent.GetMutation(category, destination))
            );
            ForcedReference = new(parent.GetForcedReferenceForCache);
            Pointer = new(parent.GetPointerForCache);
            Pair = new(first => new(first, parent));
            Array = new(count
                =>
                new(optionsId
                    =>
                    parent.GetArrayForCache(count, optionsId)
                )
            );

            Aligner = new(alignBits => new(parent, alignBits));
            FunctionInstanceType = new(() => new(parent));
            TypeType = new(() => new(parent));
            Size = new(parent.GetSizeForCache);
            SymmetricConversions = new(parent.GetSymmetricConversionsForCache);
            ArrayReferenceCache = new(id => new(parent, id));
            MissingDeclarationIssue = new(parent.GetMissingDeclarationIssueForCache);
        }
    }
}
