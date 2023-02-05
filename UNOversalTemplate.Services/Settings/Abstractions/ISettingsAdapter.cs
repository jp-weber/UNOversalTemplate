using UNOversal.Services.Serialization;

namespace UNOversal.Services.Settings
{
    public interface ISettingsAdapter
    {
        (bool successful, string result) ReadString(string key);
        void WriteString(string key, string value);
        ISerializationService SerializationService { get; }
    }
}
