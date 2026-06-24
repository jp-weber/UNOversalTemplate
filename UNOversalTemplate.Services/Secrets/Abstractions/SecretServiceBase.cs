using UNOversal.Services.Serialization;

namespace UNOversal.Services.Secrets
{
    public abstract class SecretServiceBase
    {
        static SecretServiceBase()
        {
            _helper = new SecretHelper();
        }

        static SecretHelper _helper;
        public SecretHelper Helper => _helper;

        /// <summary>
        /// Assigns the serialization service used by the desktop secret store.
        /// Call this once during DI registration (e.g. from the concrete SecretService constructor).
        /// </summary>
        public static void SetSerializer(ISerializationService serializer)
        {
#if __DESKTOP__
            _helper.Serializer = serializer;
#endif
        }
    }
}
