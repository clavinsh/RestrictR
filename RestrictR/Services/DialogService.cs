using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace RestrictR.Services
{
    public sealed class DialogService : IDialogService
    {
        public Task ShowMessageDialogAsync(string title, string message)
        {
            ContentDialog dialog = new ContentDialog();
            dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            dialog.Title = title;
            dialog.CloseButtonText = "Close";
            dialog.DefaultButton = ContentDialogButton.Close;
            dialog.Content = message;

            return dialog.ShowAsync().AsTask();
        }
    }
}
