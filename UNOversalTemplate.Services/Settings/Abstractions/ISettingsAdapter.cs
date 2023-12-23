using UNOversal.Services.Serialization;

namespace UNOversal.Services.Settings
{
    public interface ISettingsAdapter
    {
        (bool successful, string result) ReadString(string key, string containerName = "");
        void WriteString(string key, string value);
        ISerializationService SerializationService { get; }
    }
}
