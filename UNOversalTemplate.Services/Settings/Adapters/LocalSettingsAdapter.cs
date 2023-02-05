using UNOversal.Services.Serialization;
using Windows.Storage;

namespace UNOversal.Services.Settings
{
    public class LocalSettingsAdapter : ISettingsAdapter
    {
        private readonly ApplicationDataContainer _container;

        public LocalSettingsAdapter(ISerializationService serializationService)
        {
            _container = ApplicationData.Current.LocalSettings;
            SerializationService = serializationService;
        }

        public ISerializationService SerializationService { get; }


        public (bool successful, string result) ReadString(string key)
        {
            if (_container.Values.ContainsKey(key))
            {
                return (true, _container.Values[key].ToString());
            }
            else
            {
                return (false, string.Empty);
            }
        }

        public void WriteString(string key, string value)
            => _container.Values[key] = value;
    }
}
