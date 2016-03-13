namespace ReniUI.RestFul
{
    public interface IChannelRepo
    {
        string AddAndGetNewId();
        Channel Find(string key);
        void Remove(string key);
        void Update(string key, Channel value);
    }
}