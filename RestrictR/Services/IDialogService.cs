using System.Threading.Tasks;

namespace RestrictR.Services
{
    public interface IDialogService
    {
        //Shows a message dialog with a title and custom content.
        Task ShowMessageDialogAsync(string title, string message);
    }
}
