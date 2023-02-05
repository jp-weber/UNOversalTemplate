using System.Threading.Tasks;

namespace UNOversal.Services.Network
{
    public interface INetworkService 
    {
        Task<bool> GetIsInternetAvailableAsync();
        Task<bool> GetIsNetworkAvailableAsync();
    }
}