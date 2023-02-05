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

        public (bool successful, T result) Read<T>(string key)
        {
            var (successful, result) = _adapter.ReadString(key);
            if (!successful)
            {
                return (false, default(T));
            }
            return (true,_serializationService.Deserialize<T>(result));
        }

        public T SafeRead<T>(string key, T otherwise)
        {
            if (TryRead<T>(key, out var value))
            {
                return value;
            }
            else
            {
                return otherwise;
            }
        }


        public T SafeReadEnum<T>(string key, T otherwise) where T : struct
        {
            if (TryReadEnum<T>(key, out var value))
            {
                return value;
            }
            else
            {
                return otherwise;
            }
        }

        public bool TryRead<T>(string key, out T value)
        {
            try
            {
                var (successful, result) = Read<T>(key);
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
            catch (System.Exception)
            {
                value = default;
                return false;
            }
        }

        public bool TryReadEnum<T>(string key, out T value) where T : struct
        {
            try
            {
                if (TryReadString(key, out var setting))
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
            catch (System.Exception)
            {
                value = default(T);
                return false;
            }
        }

        public string ReadString(string key)
        {
            var (successful, result) = _adapter.ReadString(key);
            if (successful)
            {
                return result;
            }
            // the result contains the exception message or is empty
            return result;

        }

        public bool TryReadString(string key, out string value)
        {
            try
            {
                value = ReadString(key);
                return true;
            }
            catch (System.Exception)
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
            catch (System.Exception)
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
            catch (System.Exception)
            {
                return false;
            }
        }
    }
}
