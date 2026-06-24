using UNOversal.Services.Serialization;

namespace UNOversal.Services.Secrets
{
    public class SecretService : SecretServiceBase, ISecretService
    {
        public SecretService(ISerializationService serializationService)
        {
#if __DESKTOP__
            SetSerializer(serializationService);
#endif
        }

        public string ConnectionString
        {
            get => Helper.ReadSecret(nameof(ConnectionString));
            set => Helper.WriteSecret(nameof(ConnectionString), value);
        }
    }
}
