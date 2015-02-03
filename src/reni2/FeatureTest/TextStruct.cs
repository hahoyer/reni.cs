using System;
using System.Collections.Generic;
using System.Linq;
using hw.UnitTest;
using Reni.FeatureTest.Array;
using Reni.FeatureTest.Function;
using Reni.FeatureTest.Integer;
using Reni.FeatureTest.Text;
using Reni.FeatureTest.TypeType;

namespace Reni.FeatureTest
{
    public class TextStruct : CompilerTest
    {
        static string Definition()
        {
            return
                @"
systemdata:
{ Memory: ((0 type * ('100' to_number_of_base 256)) mutable) instance()
. !mutable FreePointer: Memory reference mutable
};

repeat: /\ ^ while() then(^ body(), repeat(^));

system: /!\
{ MaxNumber8: /!\ '7f' to_number_of_base 16 
. MaxNumber16: /!\ '7fff' to_number_of_base 16 
. MaxNumber32: /!\ '7fffffff' to_number_of_base 16 
. MaxNumber64: /!\ '7fffffffffffffff' to_number_of_base 16 

. TextItemType: /!\ MaxNumber8 text_item type 

. NewMemory: /\ 
    { result: ((^ elementType *1) reference mutable oversizeable) instance (systemdata FreePointer enable_reinterpretation) 
    . initializer: ^ initializer
    . length: ^ length
    . !mutable position: length type instance (0) 
    . repeat
    (
        while: /\ position < length,
        body: /\ 
        ( 
            result >> position := initializer(position), 
            position := (position + 1) enable_cut
        ) 
    )
    . systemdata FreePointer :=
        (systemdata FreePointer type) 
        instance 
        ((result + length) mutable enable_reinterpretation) 
    } result 
};

Text: /\
{ !mutable data: ^ reference 
. _elementType: (^ >> 0)type
. _length: ^ type / _elementType
. AfterCopy: /\ data:= system NewMemory
    ( elementType: _elementType
    . length: _length
    . initializer: /\ data >> ^
    )
. AfterCopy()
. dump_print: /!\ _data dump_print
}
";
        }

        protected override string Target => Definition() + "; " + InstanceCode + " dump_print";
        protected virtual string InstanceCode => GetStringAttribute<InstanceCodeAttribute>();
    }

    [TestFixture]
    [Output("a")]
    [InstanceCode("Text('a')")]
    [Integer1]
    [TwoFunctions]
    [FromTypeAndFunction]
    [HalloWelt]
    [ElementAccess]
    [ElementAccessVariableSetter]
    [TypeOperator]
    [DefaultInitialized]
    [FunctionVariable]
    [Repeater]
    [FunctionArgument]
    public sealed class Text1 : TextStruct {}

    [TestFixture]
    [Output("Hallo")]
    [InstanceCode("(Text('H') << 'allo'")]
    [Text1]
    public sealed class TextConcat : TextStruct {}
}