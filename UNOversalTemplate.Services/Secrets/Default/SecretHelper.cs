#if __DESKTOP__
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UNOversal.Services.Serialization;
#else
using System.Linq;
using Windows.Security.Credentials;
#endif

namespace UNOversal.Services.Secrets
{
#if __DESKTOP__
    /// <summary>
    /// Desktop (Linux/macOS/Windows-Skia) credential storage backed by an AES-GCM encrypted
    /// JSON file in the local app-data directory. The encryption key is derived from a
    /// machine+user fingerprint via PBKDF2-SHA256 so the file is bound to the current account.
    /// </summary>
    public class SecretHelper
    {
        private static readonly string _storePath = System.IO.Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData),
            "Project2FA", "secrets.dat");

        private static readonly byte[] _encKey = DeriveKey();

        private static byte[] DeriveKey()
        {
            var fingerprint = System.Environment.MachineName + "|" + System.Environment.UserName + "|Project2FA-SecretStore";
            var salt = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(fingerprint));
            return Rfc2898DeriveBytes.Pbkdf2(
                System.Text.Encoding.UTF8.GetBytes(fingerprint),
                salt,
                iterations: 100_000,
                hashAlgorithm: HashAlgorithmName.SHA256,
                outputLength: 32);
        }

        private Dictionary<string, Dictionary<string, string>> _store;

        /// <summary>
        /// AOT-safe serialization service. Set once via <see cref="SecretServiceBase.SetSerializer"/>.
        /// </summary>
        internal ISerializationService Serializer { get; set; }

        private Dictionary<string, Dictionary<string, string>> Load()
        {
            if (_store != null)
                return _store;
            try
            {
                if (!System.IO.File.Exists(_storePath))
                {
                    _store = new Dictionary<string, Dictionary<string, string>>();
                    return _store;
                }
                var raw = System.IO.File.ReadAllBytes(_storePath);
                var nonce = raw.AsSpan(0, 12);
                var tag = raw.AsSpan(12, 16);
                var cipher = raw.AsSpan(28);
                var plain = new byte[cipher.Length];
                using var aes = new AesGcm(_encKey, 16);
                aes.Decrypt(nonce, cipher, tag, plain);
                var json = System.Text.Encoding.UTF8.GetString(plain);
                _store = (Serializer?.Deserialize<Dictionary<string, Dictionary<string, string>>>(json))
                         ?? new Dictionary<string, Dictionary<string, string>>();
            }
            catch
            {
                _store = new Dictionary<string, Dictionary<string, string>>();
            }
            return _store;
        }

        private void Save()
        {
            var dir = System.IO.Path.GetDirectoryName(_storePath);
            if (!System.IO.Directory.Exists(dir))
                System.IO.Directory.CreateDirectory(dir);
            var json = Serializer?.Serialize(Load()) ?? throw new InvalidOperationException("Serializer not set on SecretHelper.");
            var plain = System.Text.Encoding.UTF8.GetBytes(json);
            var nonce = new byte[12];
            var tag = new byte[16];
            var cipher = new byte[plain.Length];
            RandomNumberGenerator.Fill(nonce);
            using var aes = new AesGcm(_encKey, 16);
            aes.Encrypt(nonce, plain, cipher, tag);
            var tmp = _storePath + ".tmp";
            using (var fs = System.IO.File.OpenWrite(tmp))
            {
                fs.Write(nonce);
                fs.Write(tag);
                fs.Write(cipher);
            }
            System.IO.File.Move(tmp, _storePath, overwrite: true);
        }

        public string ReadSecret(string key) => ReadSecret(GetType().ToString(), key);

        public string ReadSecret(string container, string key)
        {
            var store = Load();
            if (store.TryGetValue(container, out var bucket) && bucket.TryGetValue(key, out var value))
                return value;
            return string.Empty;
        }

        public void WriteSecret(string key, string secret) => WriteSecret(GetType().ToString(), key, secret);

        public void WriteSecret(string container, string key, string secret)
        {
            var store = Load();
            if (!store.TryGetValue(container, out var bucket))
            {
                bucket = new Dictionary<string, string>();
                store[container] = bucket;
            }
            bucket[key] = secret;
            Save();
        }

        public void RemoveSecret(string key) => RemoveSecret(GetType().ToString(), key);

        public void RemoveSecret(string container, string key) => IsSecretExistsForKey(container, key, true);

        public bool IsSecretExistsForKey(string key) => IsSecretExistsForKey(GetType().ToString(), key, false);

        public bool IsSecretExistsForKey(string container, string key) => IsSecretExistsForKey(container, key, false);

        private bool IsSecretExistsForKey(string container, string key, bool removeIfExists)
        {
            var store = Load();
            if (store.TryGetValue(container, out var bucket) && bucket.ContainsKey(key))
            {
                if (removeIfExists)
                {
                    bucket.Remove(key);
                    if (bucket.Count == 0)
                        store.Remove(container);
                    Save();
                }
                return true;
            }
            return false;
        }
    }
#else
    // https://msdn.microsoft.com/en-us/library/windows/apps/windows.security.credentials.passwordvault.aspx
    public class SecretHelper
    {
        static PasswordVault _vault;

        static SecretHelper()
        {
            _vault = new PasswordVault();
        }

        public string ReadSecret(string key)
        {
            return ReadSecret(GetType().ToString(), key);
        }

        public string ReadSecret(string container, string key)
        {
            if (_vault.RetrieveAll().Any(x => x.Resource == container && x.UserName == key))
            {
                var credential = _vault.Retrieve(container, key);
                credential.RetrievePassword();
                return credential.Password;
            }
            else
            {
                return string.Empty;
            }
        }

        public void WriteSecret(string key, string secret)
        {
            WriteSecret(GetType().ToString(), key, secret);
        }

        public void WriteSecret(string container, string key, string secret)
        {
            if (_vault.RetrieveAll().Any(x => x.Resource == container && x.UserName == key))
            {
                var credential = _vault.Retrieve(container, key);
                credential.RetrievePassword();
                credential.Password = secret;
                _vault.Add(credential);
            }
            else
            {
                var credential = new PasswordCredential(container, key, secret);
                _vault.Add(credential);
            }
        }

        public void RemoveSecret(string key)
        {
            RemoveSecret(GetType().ToString(), key);
        }

        public void RemoveSecret(string container, string key)
        {
            IsSecretExistsForKey(container, key, true);
        }

        public bool IsSecretExistsForKey(string key)
        {
            return IsSecretExistsForKey(GetType().ToString(), key, false);
        }

        public bool IsSecretExistsForKey(string container, string key)
        {
            return IsSecretExistsForKey(container, key, false);
        }

        private bool IsSecretExistsForKey(string container, string key, bool RemoveIfExists)
        {
            if (_vault.RetrieveAll().Any(x => x.Resource == container && x.UserName == key))
            {
                var credential = _vault.Retrieve(container, key);
                credential.RetrievePassword();
                if (credential.Password.Length > 0)
                {
                    if (RemoveIfExists)
                    {
                        _vault.Remove(credential);
                    }
                    return true;
                }
                else
                {
                    _vault.Remove(credential);
                }
            }
            return false;
        }
    }
#endif
}