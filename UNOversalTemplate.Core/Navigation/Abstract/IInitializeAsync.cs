using System.Threading.Tasks;

namespace UNOversal.Navigation
{
    public interface IInitializeAsync
    {
        Task InitializeAsync(INavigationParameters parameters);
    }
}
