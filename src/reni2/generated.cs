using System;
using HWClassLibrary.Debug;
using Reni.Runtime;

namespace Reni
{
    /// <summary>
    /// sss
    /// </summary>
    public unsafe class reni_Test
    {
        /// <summary>
        /// sss
        /// </summary>
        public static void reni()
        {
            fixed(sbyte* data = new sbyte[5])
            {
                (*(data + 4)) = (4); // BitArray 15258
                reni_0((data + 5)); // Call     14062
                (*(data + 3)) = (*(data + 4)); // TopData  15259
                (*(data + 3)) = (sbyte) ((sbyte) ((*(data + 3)) << 4) >> 4); // CreateBitCast  14158
                Data.DumpPrint((*(data + 3))); // DumpPrint  14141
                Data.MoveBytes(0, (data + 5), (data + 4)); // StatementEnd 14233
            }
            ;
        }

        // ((arg)=(1))then((arg)type(1))else((arg)*(f((arg)type(((arg)-(1))enable_cut))))
        private static void reni_0(SByte* frame)
        {
            fixed(sbyte* data = new sbyte[10])
            {
                (*(data + 9)) = (1); // BitArray     15273
                (*(data + 8)) = (*(frame - 1)); // TopFrame     15275
                (*(data + 8)) = (sbyte) ((sbyte) ((*(data + 8)) << 4) >> 4); // CreateBitCast      14522
                (*(data + 7)) = (*(data + 9)); // TopData      15276
                (*(data + 7)) = (sbyte) ((sbyte) ((*(data + 7)) << 6) >> 6); // CreateBitCast      14469
                if((*(data + 8)) == (*(data + 7)))
                {
                    ; // CreateBitArrayOpThen 15278
                    (*(data + 8)) = (1); // BitArray       15279
                    (*(data + 7)) = (*(data + 8)); // TopData        15280
                    (*(data + 7)) = (sbyte) ((sbyte) ((*(data + 7)) << 6) >> 6); // CreateBitCast      15013
                    (*(data + 8)) = (*(data + 7)); // StatementEnd 15058
                }
                else
                {
                    ; // Else         15281
                    (*(data + 8)) = (1); // BitArray    15282
                    (*(data + 7)) = (*(frame - 1)); // TopFrame    15284
                    (*(data + 7)) = (sbyte) ((sbyte) ((*(data + 7)) << 4) >> 4); // CreateBitCast     13332
                    (*(data + 6)) = (*(data + 8)); // TopData     15285
                    (*(data + 6)) = (sbyte) ((sbyte) ((*(data + 6)) << 6) >> 6); // CreateBitCast     13247
                    (*(data + 7)) = (sbyte) ((*(data + 7)) - (*(data + 6))); // CreateBitArrayOp  13387
                    (*(data + 7)) = (sbyte) ((sbyte) ((*(data + 7)) << 3) >> 3); // CreateBitCast       13388
                    (*(data + 6)) = (*(data + 7)); // TopData         15286
                    (*(data + 6)) = (sbyte) ((sbyte) ((*(data + 6)) << 3) >> 3); // CreateBitCast           13488
                    (*(data + 5)) = (*(data + 6)); // TopData             15287
                    (*(data + 5)) = (sbyte) ((sbyte) ((*(data + 5)) << 4) >> 4); // CreateBitCast              13620
                    reni_0((data + 6)); // Call                   13653
                    (*(data + 4)) = (*(frame - 1)); // TopFrame               15289
                    (*(data + 4)) = (sbyte) ((sbyte) ((*(data + 4)) << 4) >> 4); // CreateBitCast                14884
                    (*(data + 3)) = (*(data + 5)); // TopData                15290
                    (*(data + 3)) = (sbyte) ((sbyte) ((*(data + 3)) << 4) >> 4); // CreateBitCast                14827
                    (*(data + 4)) = (sbyte) ((*(data + 4))*(*(data + 3))); // CreateBitArrayOp             14934
                    (*(data + 4)) = (sbyte) ((sbyte) ((*(data + 4)) << 1) >> 1); // CreateBitCast                  14935
                    (*(data + 8)) = (*(data + 4)); // StatementEnd               15163
                }
                ; //                                                                                      EndCondional 15291
                (*(data + 9)) = (*(data + 8)); // StatementEnd                   15191
                (*(frame - 1)) = (*(data + 9)); // StorageDescriptor.FunctionReturn
            }
            ;
        }
    }
}