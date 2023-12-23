using UNOversal.Services.File;
using UNOversal.Services.Serialization;
using System;

namespace UNOversal.Services.Settings
{
    public class LocalFileSettingsAdapter : ISettingsAdapter
    {
        private readonly IFileService _helper;

        public LocalFileSettingsAdapter(ISerializationService serializationService)
        {
            _helper = new FileService(serializationService);
            SerializationService = serializationService;
        }

        public ISerializationService SerializationService { get; }


        public LocalFileSettingsAdapter(IFileService fileService)
        {
            _helper = fileService;
        }

        public (bool successful, string result) ReadString(string key, string containerName = "")
        {
            try
            {
                return (true, _helper.ReadStringAsync(key).Result);
            }
            catch (Exception exc)
            {
                return (false, exc.Message);
            }
        }

        public void WriteString(string key, string value)
        {
            if (!_helper.WriteStringAsync(key, value).Result)
            {
                throw new Exception();
            }
        }
    }
}
