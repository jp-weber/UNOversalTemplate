using System;
using UNOversal.Services.Serialization;

namespace UNOversal.Services.Settings
{
    public class SettingsHelper : ISettingsHelper
    {
        private readonly ISerializationService _serializationService;
        private readonly ISettingsAdapter _adapter;

        public SettingsHelper(ISettingsAdapter adapter)
        {
            _adapter = adapter;
            _serializationService = adapter.SerializationService;
        }

        public bool EnableCompression { get; set; } = false;

        public (bool successful, T result) Read<T>(string key, string containerName = "")
        {
            var (successful, result) = _adapter.ReadString(key, containerName);
            if (!successful)
            {
                return (false, default(T));
            }
            return (true,_serializationService.Deserialize<T>(result));
        }

        public T SafeRead<T>(string key, T otherwise, string containerName = "")
        {
            if (TryRead<T>(key, out var value, containerName))
            {
                return value;
            }
            else
            {
                return otherwise;
            }
        }


        public T SafeReadEnum<T>(string key, T otherwise, string containerName = "") where T : struct
        {
            if (TryReadEnum<T>(key, out var value, containerName))
            {
                return value;
            }
            else
            {
                return otherwise;
            }
        }

        public bool TryRead<T>(string key, out T value, string containerName = "")
        {
            try
            {
                var (successful, result) = Read<T>(key, containerName);
                if (successful)
                {
                    value = result;
                    return true;
                }
                else
                {
                    value = default;
                    return false;
                }
            }
            catch (Exception)
            {
                value = default;
                return false;
            }
        }

        public bool TryReadEnum<T>(string key, out T value, string containerName = "") where T : struct
        {
            try
            {
                if (TryReadString(key, out var setting, containerName))
                {
                    if (Enum.TryParse<T>(setting, out var result))
                    {
                        value = result;
                        return true;
                    }
                }
                value = default;
                return false;
            }
            catch (Exception)
            {
                value = default(T);
                return false;
            }
        }

        public string ReadString(string key, string containerName = "")
        {
            var (successful, result) = _adapter.ReadString(key, containerName);
            if (successful)
            {
                return result;
            }
            // the result contains the exception message or is empty
            return result;

        }

        public bool TryReadString(string key, out string value, string containerName = "")
        {
            try
            {
                value = ReadString(key, containerName);
                return true;
            }
            catch (Exception)
            {
                value = string.Empty;
                return false;
            }
        }

        public void Write<T>(string key, T value)
        {
            var result = _serializationService.Serialize(value);
            _adapter.WriteString(key, result);
        }

        public bool TryWrite<T>(string key, T value)
        {
            try
            {
                Write(key, value);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void WriteString(string key, string value)
        {
            _adapter.WriteString(key, value);
        }

        public void WriteEnum<T>(string key, T value) where T : struct
        {
            var newvalue = value.ToString();
            _adapter.WriteString(key, newvalue);
        }

        public bool TryWriteString(string key, string value)
        {
            try
            {
                WriteString(key, value);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
