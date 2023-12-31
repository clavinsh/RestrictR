using CommunityToolkit.Mvvm.DependencyInjection;
using DataPacketLibrary.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace RestrictR
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class EventListUser : Page
    {
        public ObservableCollection<Event> Events;

        private EventController _controller;

        public EventListUser()
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

        public string FormatStartDuration(Event someEvent)
        {
            return $"{someEvent.Start} - {someEvent.Start.Add(someEvent.Duration)}";
        }
    }
}
