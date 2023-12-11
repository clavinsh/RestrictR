using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestrictR.Services
{
    public interface IDialogService
    {
        //Shows a message dialog with a title and custom content.
        Task ShowMessageDialogAsync(string title, string message);
    }
}
