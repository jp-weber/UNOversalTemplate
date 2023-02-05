namespace UNOversal.Services.Secrets
{
    public interface ISecretService
    {
        string ConnectionString { get; set; }

        SecretHelper Helper { get; }
    }
}