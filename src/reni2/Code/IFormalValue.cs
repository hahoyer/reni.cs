namespace Reni.Code
{
    internal interface IFormalValue
    {
        string Dump(int index, int size);
        string Dump();
        IFormalValue RefPlus(int right);
        void Check(FormalValueAccess[] accesses);
    }
}