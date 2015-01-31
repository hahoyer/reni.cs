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
{ Memory: (0 type * ('100' to_number_of_base 256)) instance()
. FreePointer: Memory(0) raw_address
};

repeat: /\ ^ while() then(^ body(), repeat(^));

system: /!\
{ MaxNumber8: /!\ '7f' to_number_of_base 16 
. MaxNumber16: /!\ '7fff' to_number_of_base 16 
. MaxNumber32: /!\ '7fffffff' to_number_of_base 16 
. MaxNumber64: /!\ '7fffffffffffffff' to_number_of_base 16 

. TextItemType: /!\ text_item(MaxNumber8) type 

. NewMemory: /\ 
    { result: :=! ((^ elementType * MaxNumber32) instance_from_raw_address (systemdata FreePointer)) 
    . initializer: ^ initializer
    . length: ^ length
    . position: :=! length type instance (0) 
    . repeat
        ( while: ^ position < length
        . body: ^ (result(position) := initializer(position), position := position + 1) 
        )
    . systemdata FreePointer := systemdata FreePointer + (^ elementType size * ^ length)
    } result 
};

Text: /\
{ data: :=! ((system TextItemType * system MaxNumber32) instance (^ enable_array_oversize)) 
. _length: system MaxNumber32 type instance (^ type / system TextItemType)
. AfterCopy: /\ data:= system NewMemory
    ( elementType: system TextItemType
    . length: _length
    . initializer: /\ data(^)
    )
. AfterCopy()
. dump_print: /!\ _data dump_print
}
";
        }

        protected override string Target { get { return Definition() + "; " + InstanceCode + " dump_print"; } }
        protected virtual string InstanceCode { get { return GetStringAttribute<InstanceCodeAttribute>(); } }
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
    public sealed class Text1 : TextStruct
    {}

    [TestFixture]
    [Output("Hallo")]
    [InstanceCode("(Text('H') << 'allo'")]
    [Text1]
    public sealed class TextConcat : TextStruct
    {}
}