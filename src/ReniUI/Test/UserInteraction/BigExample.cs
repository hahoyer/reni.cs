using hw.UnitTest;
using ReniUI.Test.Formatting;

namespace ReniUI.Test.UserInteraction;

[UnitTest]
[Complex]
[TrainWreck]
public sealed class BigExample : DependenceProvider
{
    const string Text = @"# System definitions

#(doc
Basic things,
text class, ...
doc)#

systemdata:
{
    Memory!public :((0 type *(125)) mutable) instance();
    FreePointer! (mutable ,public) : Memory array_reference mutable;
};

repeat: @ ^ while() then(^ body(), repeat(^));

system:
{
    MaxNumber8: @! '7f' to_number_of_base 16.
    MaxNumber16: @! '7fff' to_number_of_base 16.
    MaxNumber32: @! '7fffffff' to_number_of_base 16.
    MaxNumber64: @! '7fffffffffffffff' to_number_of_base 16.
    TextItemType: @! MaxNumber8 text_item type.

    NewMemory!    public : @
    {
        result! public :
        (
            ((^ elementType) * 1)
            array_reference
            mutable
        )
        instance(systemdata FreePointer enable_reinterpretation).

        initializer: ^ initializer.
        count: ^ count.
        position!mutable : count type instance(0).

        repeat
        (
            while: @ position < count,

            body: @
            (
                result(position) := initializer(position),
                position :=(position + 1) enable_cut
            )
        ).

        systemdata FreePointer
            :=
            (systemdata FreePointer type)
                instance((result + count) mutable enable_reinterpretation)
    }
    result
};

Text: @
{
    value: ^.

    result!public :
    {
        !mix_in: data(^) := new_value @ data(^).
        this: @! ^^.
        count!public : ^ count.

        data!public :
             system
                 NewMemory
                 (
                     elementType: value(0) type.
                     count: value count.
                     initializer: @ value(^)
                 ).

        dump_print!public : @!
        {
            position!mutable : count type instance(0);

            repeat
            (
                while: @ position < count,

                body: @
                (
                    data(position) dump_print,
                    position :=(position + 1) enable_cut
                )
            )
        }.

        << !public : @ concat(count: ^ count, data: ^).

        concat: @
           Text
           (
               other: ^;
               count: this count + other count;

               !mix_in: @
                    ^ < this count
                    then this data(^ enable_cut)
                    else other data((^ - this count)enable_cut);
           )
    }
}
result
";

    const string Expected = @"# System definitions

#(doc
Basic things,
text class, ...
doc)#

systemdata:
{
    Memory!public: ((0 type *(125)) mutable) instance();
    FreePointer!(mutable, public): Memory array_reference mutable;
};

repeat: @ ^ while() then(^ body(), repeat(^));

system:
{
    MaxNumber8: @! '7f' to_number_of_base 16.
    MaxNumber16: @! '7fff' to_number_of_base 16.
    MaxNumber32: @! '7fffffff' to_number_of_base 16.
    MaxNumber64: @! '7fffffffffffffff' to_number_of_base 16.
    TextItemType: @! MaxNumber8 text_item type.

    NewMemory!public: @
        {
            result!public:
                (
                    ((^ elementType) * 1)
                    array_reference
                    mutable
                )
                instance(systemdata FreePointer enable_reinterpretation).

            initializer: ^ initializer.
            count: ^ count.
            position!mutable: count type instance(0).

            repeat
            (
                while: @ position < count,

                body: @
                (
                    result(position) := initializer(position),
                    position :=(position + 1) enable_cut
                )
            ).

            systemdata
            FreePointer
            :=
                (systemdata FreePointer type)
                instance((result + count) mutable enable_reinterpretation)
        }
        result
};

Text: @
    {
        value: ^.

        result!public:
        {
            !mix_in: data(^) := new_value @ data(^).
            this: @! ^^.
            count!public: ^ count.

            data!public:
                system
                NewMemory
                (
                    elementType: value(0) type.
                    count: value count.
                    initializer: @ value(^)
                ).

            dump_print!public: @!
            {
                position!mutable: count type instance(0);

                repeat
                (
                    while: @ position < count,

                    body: @
                    (
                        data(position) dump_print,
                        position :=(position + 1) enable_cut
                    )
                )
            }.

            <<!public: @ concat(count: ^ count, data: ^).

            concat: @
                Text
                (
                    other: ^;
                    count: this count + other count;

                    !mix_in: @
                        ^ < this count
                        then this data(^ enable_cut)
                        else other data((^ - this count) enable_cut);
                )
        }
    }
    result
";

    [UnitTest]
    public void Reformat() => Text.SimpleFormattingTest(Expected, indentCount: 4);
}