using CommunityToolkit.Mvvm.DependencyInjection;
using DataPacketLibrary.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace RestrictR
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class EventList : Page
    {

        public ObservableCollection<Event> Events;

        private EventController _controller;

        public EventList()
        {
            this.InitializeComponent();
            _controller = Ioc.Default.GetRequiredService<EventController>();

            Events = new ObservableCollection<Event>();

            Loaded += EventList_Loaded;
        }

        private async void EventList_Loaded(object sender, RoutedEventArgs e)
        {
            var eventsList = await _controller.GetEvents();

            foreach (var ev in eventsList)
            {
                Events.Add(ev);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.MainFrame.Navigate(typeof(EventForm));
        }

        public string FormatStartDuration(Event someEvent)
        {
            return $"{someEvent.Start} - {someEvent.Start.Add(someEvent.Duration)}";
        }
    }
}
