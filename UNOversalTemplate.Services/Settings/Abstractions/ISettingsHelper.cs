namespace UNOversal.Services.Settings
{
    public interface ISettingsHelper
    {
        //bool EnableCompression { get; set; }

        (bool successful, T result) Read<T>(string key, string containerName = "");
        string ReadString(string key, string containerName = "");
        T SafeRead<T>(string key, T otherwise, string containerName = "");
        bool TryRead<T>(string key, out T value, string containerName = "");
        bool TryReadString(string key, out string value, string containerName = "");

        bool TryWrite<T>(string key, T value);
        bool TryWriteString(string key, string value);
        void Write<T>(string key, T value);
        void WriteString(string key, string value);
    }
}