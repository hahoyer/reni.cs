namespace ReniUI.RestFul
{
    public interface IChannelRepo
    {
        Channel Find(string key);
        void Remove(string key);
        void Update(string key, Channel value);
        string Create(Channel value);
    }
}