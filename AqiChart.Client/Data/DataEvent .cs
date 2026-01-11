
namespace AqiChart.Client.Data
{
    public class DataEvent
    {
        public DataEvent(string key, object data)
        {
            Key = key;
            Data = data;
        }
        public string Key { get; set; }

        public object Data { get; set; }

    }
}
