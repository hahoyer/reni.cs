using System.Collections.Generic;
using hw.DebugFormatter;

namespace ReniUI.RestFul
{
    public sealed class ChannelRepo : DumpableObject, IChannelRepo
    {
        int _nextKey = 1;
        string CreateNewKey() => _nextKey++.ToString();

        readonly IDictionary<string, Channel> Data = new Dictionary<string, Channel>();

        Channel IChannelRepo.Find(string key) => Data.ContainsKey(key) ? Data[key] : null;
        void IChannelRepo.Remove(string key) => Data.Remove(key);
        void IChannelRepo.Update(string key, Channel value) { Data[key].Text = value.Text; }

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
    }
}