using System;
using System.Collections.Generic;
using System.Linq;
using hw.UnitTest;
using Reni.FeatureTest.Array;
using Reni.FeatureTest.BlogExamples;
using Reni.FeatureTest.Function;
using Reni.FeatureTest.Helper;
using Reni.FeatureTest.Integer;
using Reni.FeatureTest.Reference;
using Reni.FeatureTest.Text;
using Reni.FeatureTest.TypeType;
using Reni.FeatureTest.Validation;

namespace Reni.FeatureTest
{
    public class TextStruct : CompilerTest
    {
        static string Definition() => @"
systemdata:
{ 
    Memory: ((0 type * ('100' to_number_of_base 64)) mutable) instance();
    !mutable FreePointer: Memory array_reference mutable;
};

repeat: /\ ^ while() then(^ body(), repeat(^));

system: 
{ MaxNumber8: /!\ '7f' to_number_of_base 16 
. MaxNumber16: /!\ '7fff' to_number_of_base 16 
. MaxNumber32: /!\ '7fffffff' to_number_of_base 16 
. MaxNumber64: /!\ '7fffffffffffffff' to_number_of_base 16 

. TextItemType: /!\ MaxNumber8 text_item type 

. NewMemory: /\ 
    { result: (((^ elementType) * 1) array_reference mutable) instance (systemdata FreePointer enable_reinterpretation) 
    . initializer: ^ initializer
    . count: ^ count
    . !mutable position: count type instance (0) 
    . repeat
    (
        while: /\ position < count,
        body: /\ 
        ( 
            result item(position) := initializer(position), 
            position := (position + 1) enable_cut
        ) 
    )
    . systemdata FreePointer :=
        (systemdata FreePointer type) 
        instance 
        ((result + count) mutable enable_reinterpretation) 
    } result 
};

Text: /\
{ _elementType: ^ type item
. !mutable data: ((_elementType*1) array_reference)instance(^)
. _count: ^ count
. AfterCopy: /\ data:= system NewMemory
    ( elementType: _elementType
    . count: _count
    . initializer: /\ data item(^)
    )
. AfterCopy()
. dump_print: /!\ 
    {
        !mutable position: _count type instance(0) ;
        repeat
        (
            while: /\ position < _count,
            body: /\ 
            ( 
                data item(position) dump_print, 
                position := (position + 1) enable_cut
            ) 
        )
    }
. << : /\ 
    {
        
    } 
}
";

        protected override string Target => Definition() + "; (" + InstanceCode + ") dump_print";
        protected virtual string InstanceCode => GetStringAttribute<InstanceCodeAttribute>();
    }

    [UnitTest]
    [Output("abcdef")]
    [InstanceCode("Text('abcdef')")]
    [Integer1]
    [TwoFunctions]
    [FromTypeAndFunction]
    [HalloWelt]
    [ElementAccess]
    [ElementAccessVariableSetter]
    [TypeOperator]
    [DefaultInitialized]
    [FunctionVariable]
    [WikiExamples]
    [Repeater]
    [FunctionArgument]
    [PrimitiveRecursiveFunctionHuge]
    [ArrayElementType]
    [ArrayReferenceAll]
    public sealed class Text1 : TextStruct {}

    [UnitTest]
    [Output("Hallo")]
    [InstanceCode("Text('H') << 'allo'")]
    [Text1]
    [UserObjects]
    [UnMatchedBrackets]
    [LowPriority]
    public sealed class TextConcat : TextStruct {}
}