using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace UNOversal.Services.File
{
    public class FileService : IFileService
    {
        Serialization.ISerializationService _serializer;
        public FileService(Serialization.ISerializationService serializer)
        {
            _serializer = serializer;
        }

        /// <summary>Deletes a file in the specified storage strategy</summary>
        /// <param name="key">Path of the file in storage</param>
        /// <param name="location">Location storage strategy</param>
        /// <param name="path">Custom path for storage. Only accessible if rights have been granted</param>
        /// <returns>Boolean: true if deleted, false if not deleted</returns>
        public async Task<bool> DeleteFileAsync(string key, StorageStrategies location = StorageStrategies.Local, string path = null)
        {
            var file = await GetIfFileExistsAsync(key, location, path);
            if (file != null)
                await file.DeleteAsync();
            return !(await FileExistsAsync(key, location, path));
        }

        /// <summary>Deletes a file in the specified folder/summary>
        /// <param name="key">Path of the file in storage</param>
        /// <param name="folder">Parent folder of the file. Only accessible if rights have been granted</param>
        /// <returns>Boolean: true if deleted, false if not deleted</returns>
        public async Task<bool> DeleteFileAsync(string key, StorageFolder folder)
        {
            var file = await GetIfFileExistsAsync(key, folder);
            if (file != null)
                await file.DeleteAsync();
            return !(await FileExistsAsync(key, folder));
        }

        /// <summary>Returns if a file is found in the specified storage strategy</summary>
        /// <param name="key">Path of the file in storage</param>
        /// <param name="location">Location storage strategy</param>
        /// <param name="path">Custom path for storage. Only accessible if rights have been granted</param>
        /// <returns>Boolean: true if found, false if not found</returns>
        public async Task<bool> FileExistsAsync(string key, StorageStrategies location = StorageStrategies.Local, string path = null)
            => (await GetIfFileExistsAsync(key, location, path)) != null;

        /// <summary>Returns if a file is found in the specified folder</summary>
        /// <param name="key">Path of the file in storage</param>
        /// <param name="folder">Parent folder of the file. Only accessible if rights have been granted</param>
        /// <returns>Boolean: true if found, false if not found</returns>
        public async Task<bool> FileExistsAsync(string key, StorageFolder folder) => (await GetIfFileExistsAsync(key, folder)) != null;

        /// <summary>Reads and deserializes a file into specified type T</summary>
        /// <typeparam name="T">Specified type into which to deserialize file content</typeparam>
        /// <param name="key">Path to the file in storage</param>
        /// <param name="location">Location storage strategy</param>
        /// <param name="path">Custom path for storage (only accessible if rights have been granted)</param>
        /// <returns>Specified type T</returns>
        public async Task<T> ReadFileAsync<T>(string key, StorageStrategies location = StorageStrategies.Local, string path = null)
        {
            try
            {
                // fetch file
                var file = await GetIfFileExistsAsync(key, location, path);
                if (file == null)
                    return default(T);
                // read content
                var readValue = await FileIO.ReadTextAsync(file);
                // convert to obj
                var _Result = Deserialize<T>(readValue);
                return _Result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>Reads and deserializes a file into specified type T</summary>
        /// <typeparam name="T">Specified type into which to deserialize file content</typeparam>
        /// <param name="key">Path to the file in storage</param>
        /// <param name="folder">Parent folder of the file. Only accessible if rights have been granted</param>
        /// <returns>Specified type T</returns>
        public async Task<T> ReadFileAsync<T>(string key, StorageFolder folder)
        {
            try
            {
                // fetch file
                var file = await GetIfFileExistsAsync(key, folder);
                if (file == null)
                    return default(T);
                // read content
                var readValue = await FileIO.ReadTextAsync(file);
                // convert to obj
                var _Result = Deserialize<T>(readValue);
                return _Result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>Reads and deserializes a file as string</summary>
        /// <param name="key">Path to the file in storage</param>
        /// <param name="location">Location storage strategy</param>
        /// <param name="path">Custom path for storage. Only accessible if rights have been granted</param>
        /// <returns>string</returns>
        public async Task<string> ReadStringAsync(string key, StorageStrategies location = StorageStrategies.Local, string path = null)
        {
            try
            {
                // fetch file
                var file = await GetIfFileExistsAsync(key, location, path);
                if (file == null)
                    return string.Empty;
                // read content
                return await FileIO.ReadTextAsync(file);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>Reads and deserializes a file as string</summary>
        /// <param name="key">Path to the file in storage</param>
        /// <param name="folder">Parent folder of the file. Only accessible if rights have been granted</param>
        /// <returns>string</returns>
        public async Task<string> ReadStringAsync(string key, StorageFolder folder)
        {
            try
            {
                // fetch file
                var file = await GetIfFileExistsAsync(key, folder);
                if (file == null)
                    return string.Empty;
                // read content
                return await FileIO.ReadTextAsync(file);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>Serializes an object and write to file in specified storage strategy</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">Path to the file in storage</param>
        /// <param name="value">Instance of object to be serialized and written</param>
        /// <param name="location">Location storage strategy</param>
        /// <param name="option">defines the collision handling</param>
        /// <param name="path">Custom path for storage. Only accessible if rights have been granted</param>
        /// <returns></returns>
        public async Task<bool> WriteFileAsync<T>(string key, T value, StorageStrategies location = StorageStrategies.Local,
            CreationCollisionOption option = CreationCollisionOption.ReplaceExisting, string path = null)
        {
            // create file
            var file = await CreateFileAsync(key, location, option, path);
            // convert to string
            var serializedValue = Serialize(value);
            // save string to file
            await FileIO.WriteTextAsync(file, serializedValue);
            // result
            return await FileExistsAsync(key, location, path);
        }

        /// <summary>Serializes an object and write to file in specified storage strategy</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">Path to the file in storage</param>
        /// <param name="value">Instance of object to be serialized and written</param>
        /// <param name="folder">Parent folder of the file. Only accessible if rights have been granted</param>
        /// <returns></returns>
        public async Task<bool> WriteFileAsync<T>(string key, T value, StorageFolder folder)
        {
            // create file
            var file = await CreateFileAsync(key, folder);
            // convert to string
            var serializedValue = Serialize(value);
            // save string to file
            await FileIO.WriteTextAsync(file, serializedValue);
            // result
            return await FileExistsAsync(key, folder);
        }

        /// <summary>Write string to file in specified storage strategy</summary>
        /// <param name="key">Path to the file in storage</param>
        /// <param name="value">Instance of object to be serialized and written</param>
        /// <param name="option">defines the collision handling</param>
        /// <param name="path">Custom path for storage. Only accessible if rights have been granted</param>
        /// <returns></returns>
        public async Task<bool> WriteStringAsync(string key, string value, StorageStrategies location = StorageStrategies.Local,
            CreationCollisionOption option = CreationCollisionOption.ReplaceExisting, string path = null)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentNullException();
            }
            // create file
            var file = await CreateFileAsync(key, location, option, path);
            // save string to file
            await FileIO.WriteTextAsync(file, value);
            // result
            return await FileExistsAsync(key, location, path);
        }

        /// <summary>Write string to file in specified storage strategy</summary>
        /// <param name="key">Path to the file in storage</param>
        /// <param name="value">Instance of object to be serialized and written</param>
        /// <param name="folder">Parent folder of the file. Only accessible if rights have been granted</param>
        /// <returns></returns>
        public async Task<bool> WriteStringAsync(string key, string value, StorageFolder folder)
        {
            // create file
            var file = await CreateFileAsync(key, folder);
            // save string to file
            await FileIO.WriteTextAsync(file, value);
            // result
            return await FileExistsAsync(key, folder);
        }

        /// <summary>
        /// Creates a file with the specified key in the given storage location, using the provided creation option.
        /// </summary>
        /// <remarks>When using the Custom storage location, ensure that the path parameter is provided
        /// and valid. For other locations, the path parameter is ignored.</remarks>
        /// <param name="key">The name of the file to create. Cannot be null or empty.</param>
        /// <param name="location">The storage location where the file will be created. Defaults to Local if not specified.</param>
        /// <param name="option">The option that determines how to handle the case when a file with the same name already exists.</param>
        /// <param name="path">The base path to use when the storage location is Custom. Required if location is set to Custom; otherwise,
        /// ignored.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the created or opened
        /// StorageFile instance.</returns>
        /// <exception cref="NotSupportedException">Thrown if the specified storage location is not supported.</exception>
        private async Task<StorageFile> CreateFileAsync(string key, StorageStrategies location = StorageStrategies.Local,
            CreationCollisionOption option = CreationCollisionOption.OpenIfExists, string path = null)
        {
            switch (location)
            {
                case StorageStrategies.Local:
                    return await ApplicationData.Current.LocalFolder.CreateFileAsync(key, option);
                case StorageStrategies.Roaming:
                    return await ApplicationData.Current.RoamingFolder.CreateFileAsync(key, option);
                case StorageStrategies.Temporary:
                    return await ApplicationData.Current.TemporaryFolder.CreateFileAsync(key, option);
                case StorageStrategies.Custom:
                    //only using, if a filepicker is used. In other case, use CreateFileAsync(string key, StorageFolder folder,...)
                    return await StorageFile.GetFileFromPathAsync(path + key);
                default:
                    throw new NotSupportedException(location.ToString());
            }
        }

        /// <summary>
        /// Asynchronously creates a new file with the specified name in the given folder, or opens it if it already
        /// exists, using the specified collision option.
        /// </summary>
        /// <param name="key">The name of the file to create. Cannot be null or empty.</param>
        /// <param name="folder">The folder in which to create the file. Cannot be null.</param>
        /// <param name="option">A value that determines how to handle the case when a file with the same name already exists. The default is
        /// OpenIfExists.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a StorageFile that represents
        /// the created or opened file.</returns>
        private async Task<StorageFile> CreateFileAsync(string key, 
            StorageFolder folder, CreationCollisionOption option = CreationCollisionOption.OpenIfExists)
        {
            return await folder.CreateFileAsync(key, option);
        }

        /// <summary>
        /// Attempts to retrieve a file with the specified name from the given folder asynchronously, returning null if
        /// the file does not exist.
        /// </summary>
        /// <param name="key">The name of the file to retrieve from the folder. Cannot be null or empty.</param>
        /// <param name="folder">The folder in which to search for the file. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the file if it exists;
        /// otherwise, null.</returns>
        private async Task<StorageFile?> GetIfFileExistsAsync(string key, StorageFolder folder)
        {
            StorageFile retval;
            try
            {
                if (folder != null)
                {
                    retval = await folder.GetFileAsync(key);
                }
                else
                {
                    return null;
                }
                
            }
            catch (System.IO.FileNotFoundException)
            {
                System.Diagnostics.Debug.WriteLine(nameof(GetIfFileExistsAsync) + ":FileNotFoundException");
                return null;
            }
            return retval;
        }

        /// <summary>Returns a file if it is found in the specified storage strategy</summary>
        /// <param name="key">Path of the file in storage</param>
        /// <param name="location">Location storage strategy</param>
        /// <param name="path">Custom path for storage (only accessible if rights have been granted)</param>
        /// <returns>StorageFile</returns>
        private async Task<StorageFile> GetIfFileExistsAsync(string key,
            StorageStrategies location = StorageStrategies.Local, string path = null)
        {
            StorageFile retval;
            try
            {
                switch (location)
                {
                    case StorageStrategies.Local:
                        retval = await ApplicationData.Current.LocalFolder.TryGetItemAsync(key) as StorageFile;
                        break;
                    case StorageStrategies.Roaming:
                        retval = await ApplicationData.Current.RoamingFolder.TryGetItemAsync(key) as StorageFile;
                        break;
                    case StorageStrategies.Temporary:
                        retval = await ApplicationData.Current.TemporaryFolder.TryGetItemAsync(key) as StorageFile;
                        break;
                    case StorageStrategies.Custom:
                        //retval = await (await StorageFolder.GetFolderFromPathAsync(path)).GetFileAsync(key);
                        retval = await StorageFile.GetFileFromPathAsync(path + key);
                        break;
                    default:
                        throw new NotSupportedException(location.ToString());
                }
            }
            catch (System.IO.FileNotFoundException)
            {
                System.Diagnostics.Debug.WriteLine("GetIfFileExistsAsync:FileNotFoundException");
                return null;
            }
            return retval;
        }

        /// <summary>
        /// Serializes the specified object to a string representation.
        /// </summary>
        /// <typeparam name="T">The type of the object to serialize.</typeparam>
        /// <param name="item">The object to serialize. Cannot be null.</param>
        /// <returns>A string that represents the serialized form of the specified object.</returns>
        private string Serialize<T>(T item)
            => _serializer.Serialize(item);

        /// <summary>
        /// Deserializes the specified JSON string to an object of type T.
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize to.</typeparam>
        /// <param name="json">The JSON string to deserialize. Cannot be null.</param>
        /// <returns>An instance of type T that represents the deserialized JSON data.</returns>
        private T Deserialize<T>(string json)
            => _serializer.Deserialize<T>(json);
    }
}
