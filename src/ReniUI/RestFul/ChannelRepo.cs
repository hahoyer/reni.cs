namespace ReniUI.RestFul;

public sealed class ChannelRepo : DumpableObject, IChannelRepo
{
    readonly IDictionary<string, Channel> Data = new Dictionary<string, Channel>();
    int NextKey = 1;

    string IChannelRepo.Create(Channel value)
    {
        var result = CreateNewKey();
        var channel = new Channel
        {
            Text = value.Text
        };

        Data.Add(result, channel);
        return result;
    }

    Channel IChannelRepo.Find(string key) => Data.ContainsKey(key)? Data[key] : null;
    void IChannelRepo.Remove(string key) => Data.Remove(key);
    void IChannelRepo.Update(string key, Channel value) => Data[key].Text = value.Text;
    string CreateNewKey() => NextKey++.ToString();
}