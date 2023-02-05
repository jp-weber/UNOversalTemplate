using System.Threading.Tasks;

namespace UNOversal.Services.Dialogs
{
    public interface IDialogInitializeAsync
    {
        Task InitializeAsync(IDialogParameters parameters);
    }
}
