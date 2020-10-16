using System;
using System.Collections.Generic;
using System.Linq;
using Reni.Runtime;

namespace ReniTest
{
    public static class T4CompilerGenerated
    {
        // 
        //systemdata:
        //{ 
        //    Memory: ((0 type * ('100' to_number_of_base 64)) mutable) instance();
        //    !mutable FreePointer: Memory array_reference mutable;
        //    FreePointer >> 0 := (-1) enable_cut;
        //    FreePointer >> 1 := (-1) enable_cut;
        //    FreePointer >> 2 := (-1) enable_cut;
        //    FreePointer >> 3 := (-1) enable_cut;
        //    FreePointer >> 4 := (-1) enable_cut;
        //    FreePointer >> 5 := (-1) enable_cut;
        //    FreePointer >> 6 := (-1) enable_cut;
        //    FreePointer >> 7 := (-1) enable_cut;
        //    FreePointer >> 64 := (-1) enable_cut
        //};
        //
        //repeat: @ ^ while() then(^ body(), repeat(^));
        //
        //system: @!
        //{ MaxNumber8: @! '7f' to_number_of_base 16 
        //. MaxNumber16: @! '7fff' to_number_of_base 16 
        //. MaxNumber32: @! '7fffffff' to_number_of_base 16 
        //. MaxNumber64: @! '7fffffffffffffff' to_number_of_base 16 
        //
        //. TextItemType: @! MaxNumber8 text_item type 
        //
        //. NewMemory: @ 
        //    { result: (((^ elementType) * 1) array_reference mutable) instance (systemdata FreePointer enable_reinterpretation) 
        //    . initializer: ^ initializer
        //    . count: ^ count
        //    . !mutable position: count type instance (0) 
        //    . repeat
        //    (
        //        while: @ position < count,
        //        body: @ 
        //        ( 
        //            (result >> position) := initializer(position), 
        //            position := (position + 1) enable_cut
        //        ) 
        //    )
        //    . systemdata FreePointer :=
        //        (systemdata FreePointer type) 
        //        instance 
        //        ((result + count) mutable enable_reinterpretation) 
        //    } result 
        //};
        //
        //Text: @
        //{ !mutable data: ^ array_reference 
        //. _elementType: ^ type >>
        //. _count: ^ count
        //. AfterCopy: @ data:= system NewMemory
        //    ( elementType: _elementType
        //    . count: _count
        //    . initializer: @ data >> ^
        //    )
        //. AfterCopy()
        //. dump_print: @! 
        //    {
        //        !mutable position: _count type instance (0) ;
        //        repeat
        //        (
        //            while: @ position < _count,
        //            body: @ 
        //            ( 
        //                (data >> position) dump_print, 
        //                position := (position + 1) enable_cut
        //            ) 
        //        )
        //    }
        //}
        //; Text('abcdef') dump_print 

        sealed class MyClass
        {
            public Data.IView Memory;
            public Data.IView FreePointer;
            public Data.IView Arg;
            public Data.IView Arg_Base;
            public Data.IView Arg_Pointer;
        }

        static readonly MyClass _views = new MyClass();

        unsafe static public void MainFunction()
        {
            var data = Data.Create(4115);
            data.SizedPush(4096, 0);
            _views.Memory = data.GetCurrentView(4096);
            data.Push(data.Pointer(0));
            _views.FreePointer = data.GetCurrentView(4);
            data.SizedPush(6, 97, 98, 99, 100, 101, 102);
            _views.Arg = data.GetCurrentView(6);
            data.Push(data.Pointer(6));
            _views.Arg_Base = data.GetCurrentView(4);
            data.Push(data.Pointer(4));
            _views.Arg_Pointer = data.GetCurrentView(4);
            data.Push(GetFunction0(data.Pull(8)));
            data.Push(data.Pointer(0));
            data.Push(GetFunction8(data.Pull(4)));
            data.Drop(12, 1);

        }

        // { !mutable data: ^ array_reference 
        //. _elementType: ^ type >>
        //. _count: ^ count
        //. AfterCopy: @ data:= system NewMemory
        //    ( elementType: _elementType
        //    . count: _count
        //    . initializer: @ data >> ^
        //    )
        //. AfterCopy()
        //. dump_print: @! 
        //    {
        //        !mutable position: _count type instance (0) ;
        //        repeat
        //        (
        //            while: @ position < _count,
        //            body: @ 
        //            ( 
        //                (data >> position) dump_print, 
        //                position := (position + 1) enable_cut
        //            ) 
        //        )
        //    }
        //} 
        static Data GetFunction0(Data frame)
        {
            var data = Data.Create(13);
            data.Push(frame.Get(4, 0));
            data.SizedPush(1, 6);
            data.Push(frame.Get(4, 4));
            data.Push(data.Pointer(4));
            data.Push(GetFunction1(data.Pull(8)));

            return data;
        }

        // data:= system NewMemory
        //    ( elementType: _elementType
        //    . count: _count
        //    . initializer: @ data >> ^
        //    ) 
        static Data GetFunction1(Data frame)
        {
            var data = Data.Create(17);
            data.Push(frame.Get(4, 0));
            data.Push(data.Pull(4).DePointer(1));
            data.Push(frame.Get(4, 0));
            data.PointerPlus(1);
            data.Push(frame.Get(4, 4));
            data.Push(frame.Get(4, 0));
            data.Push(data.Pointer(12));
            data.Push(GetFunction3(data.Pull(12)));
            data.Assign(4);
            data.Drop(1);

            return data;
        }

        // { MaxNumber8: @! '7f' to_number_of_base 16 
        //. MaxNumber16: @! '7fff' to_number_of_base 16 
        //. MaxNumber32: @! '7fffffff' to_number_of_base 16 
        //. MaxNumber64: @! '7fffffffffffffff' to_number_of_base 16 
        //
        //. TextItemType: @! MaxNumber8 text_item type 
        //
        //. NewMemory: @ 
        //    { result: (((^ elementType) * 1) array_reference mutable) instance (systemdata FreePointer enable_reinterpretation) 
        //    . initializer: ^ initializer
        //    . count: ^ count
        //    . !mutable position: count type instance (0) 
        //    . repeat
        //    (
        //        while: @ position < count,
        //        body: @ 
        //        ( 
        //            (result >> position) := initializer(position), 
        //            position := (position + 1) enable_cut
        //        ) 
        //    )
        //    . systemdata FreePointer :=
        //        (systemdata FreePointer type) 
        //        instance 
        //        ((result + count) mutable enable_reinterpretation) 
        //    } result 
        //} 
        static Data GetFunction2(Data frame)
        {
            var data = Data.Create(0);
            data.SizedPush(0);

            return data;
        }

        // { result: (((^ elementType) * 1) array_reference mutable) instance (systemdata FreePointer enable_reinterpretation) 
        //    . initializer: ^ initializer
        //    . count: ^ count
        //    . !mutable position: count type instance (0) 
        //    . repeat
        //    (
        //        while: @ position < count,
        //        body: @ 
        //        ( 
        //            (result >> position) := initializer(position), 
        //            position := (position + 1) enable_cut
        //        ) 
        //    )
        //    . systemdata FreePointer :=
        //        (systemdata FreePointer type) 
        //        instance 
        //        ((result + count) mutable enable_reinterpretation) 
        //    } result 
        static Data GetFunction3(Data frame)
        {
            var data = Data.Create(18);
            data.Push(frame.Get(4, 8));
            data.Push(data.Pull(4).DePointer(4));
            data.Push(frame.Get(4, 0));
            data.Push(data.Pull(4).DePointer(1));
            data.SizedPush(1, 0);
            data.Push(data.Pointer(0));
            data.Push(frame.Get(4, 4));
            data.Push(GetFunction4(data.Pull(8)));
            data.Push(frame.Get(4, 8));
            data.Push(data.Get(4, 6));
            data.Push(data.Get(1, 9).BitCast(8).BitCast(32));
            data.Plus(4, 4, 4);
            data.Assign(4);
            data.Push(data.Get(4, 2));
            data.Drop(10, 4);

            return data;
        }

        // ^ while() then(^ body(), repeat(^)) 
        static Data GetFunction4(Data frame)
        {
            Start:

            var data = Data.Create(8);
            data.Push(frame.Get(4, 4));
            data.Push(GetFunction7(data.Pull(4)));
            data.Push(data.Pull(1).BitCast(1).BitCast(8));
            if(data.Pull(1).GetBytes()[0] != 0)
            {
                ;
                data.Push(frame.Get(4, 0));
                data.Push(frame.Get(4, 4));
                data.Push(GetFunction5(data.Pull(8)));
                goto Start;
            }
            ;
            data.SizedPush(0);
            ;

            return data;
        }

        // ( 
        //            (result >> position) := initializer(position), 
        //            position := (position + 1) enable_cut
        //        ) 
        static Data GetFunction5(Data frame)
        {
            var data = Data.Create(9);
            data.Push(frame.Get(4, 4));
            data.Push(frame.Get(4, 0));
            data.Push(GetFunction6(data.Pull(8)));
            data.Push(frame.Get(4, 0));
            data.PointerPlus(2);
            data.Push(data.Pull(4).DePointer(4));
            data.Push(frame.Get(4, 0));
            data.Push(data.Pull(4).DePointer(1).BitCast(8).BitCast(32));
            data.Plus(4, 4, 4);
            data.Push(data.Pointer(4));
            data.Assign(1);
            data.Drop(1);
            data.Push(frame.Get(4, 0));
            data.Push(data.Pull(4).DePointer(1));
            data.SizedPush(1, 1);
            data.Plus(1, 1, 1);
            data.Push(data.Pull(1).BitCast(9).BitCast(8));
            data.Push(frame.Get(4, 0));
            data.Push(data.Pointer(4));
            data.Assign(1);
            data.Drop(1);

            return data;
        }

        // data >> ^ 
        static Data GetFunction6(Data frame)
        {
            var data = Data.Create(8);
            data.Push(frame.Get(4, 4));
            data.PointerPlus(1);
            data.Push(data.Pull(4).DePointer(4));
            data.Push(frame.Get(4, 0));
            data.Push(data.Pull(4).DePointer(1).BitCast(8).BitCast(32));
            data.Plus(4, 4, 4);
            data.Push(data.Pull(4).DePointer(1));

            return data;
        }

        // position < count 
        static Data GetFunction7(Data frame)
        {
            var data = Data.Create(5);
            data.Push(frame.Get(4, 0));
            data.Push(data.Pull(4).DePointer(1));
            data.Push(frame.Get(4, 0));
            data.PointerPlus(1);
            data.Push(data.Pull(4).DePointer(1));
            data.Less(1, 1, 1);
            data.Push(data.Pull(1).BitCast(1).BitCast(8));

            return data;
        }

        // {
        //        !mutable position: _count type instance (0) ;
        //        repeat
        //        (
        //            while: @ position < _count,
        //            body: @ 
        //            ( 
        //                (data >> position) dump_print, 
        //                position := (position + 1) enable_cut
        //            ) 
        //        )
        //    } 
        static Data GetFunction8(Data frame)
        {
            var data = Data.Create(9);
            data.SizedPush(1, 0);
            data.Push(frame.Get(4, 0));
            data.Push(data.Pointer(4));
            data.Push(GetFunction9(data.Pull(8)));

            return data;
        }

        // ^ while() then(^ body(), repeat(^)) 
        static Data GetFunction9(Data frame)
        {
            Start:

            var data = Data.Create(8);
            data.Push(frame.Get(4, 4));
            data.Push(frame.Get(4, 0));
            data.Push(GetFunction11(data.Pull(8)));
            data.Push(data.Pull(1).BitCast(1).BitCast(8));
            if(data.Pull(1).GetBytes()[0] != 0)
            {
                ;
                data.Push(frame.Get(4, 4));
                data.Push(frame.Get(4, 0));
                data.Push(GetFunction10(data.Pull(8)));
                goto Start;
            }
            ;
            data.SizedPush(0);
            ;

            return data;
        }

        // ( 
        //                (data >> position) dump_print, 
        //                position := (position + 1) enable_cut
        //            ) 
        static Data GetFunction10(Data frame)
        {
            var data = Data.Create(9);
            data.Push(frame.Get(4, 4));
            data.PointerPlus(1);
            data.Push(data.Pull(4).DePointer(4));
            data.Push(frame.Get(4, 0));
            data.Push(data.Pull(4).DePointer(1).BitCast(8).BitCast(32));
            data.Plus(4, 4, 4);
            data.Push(data.Pull(4).DePointer(1));
            data.Pull(1).PrintText(1);
            data.Push(frame.Get(4, 0));
            data.Push(data.Pull(4).DePointer(1));
            data.SizedPush(1, 1);
            data.Plus(1, 1, 1);
            data.Push(data.Pull(1).BitCast(9).BitCast(8));
            data.Push(frame.Get(4, 0));
            data.Push(data.Pointer(4));
            data.Assign(1);
            data.Drop(1);

            return data;
        }

        // position < _count 
        static Data GetFunction11(Data frame)
        {
            var data = Data.Create(5);
            data.Push(frame.Get(4, 0));
            data.Push(data.Pull(4).DePointer(1));
            data.Push(frame.Get(4, 4));
            data.Push(data.Pull(4).DePointer(1));
            data.Less(1, 1, 1);
            data.Push(data.Pull(1).BitCast(1).BitCast(8));

            return data;
        }
    }
}
