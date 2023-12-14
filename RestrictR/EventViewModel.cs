using DataPacketLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;
using RestrictR.ValidationAttributes;
using CommunityToolkit.Mvvm.Input;
using RestrictR.Services;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using Windows.System;
using Microsoft.UI.Xaml;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace RestrictR
{
    public partial class EventViewModel : ObservableValidator
    {
        public List<ApplicationInfo> Apps = new();

        public ObservableCollection<ApplicationInfo> AppsFiltered;

        public ObservableCollection<ApplicationInfo> BlockedApplications { get; private set; } = new ObservableCollection<ApplicationInfo>();

        public EventViewModel()
        {
            ErrorsChanged += SetValidationErrors;

            _startDate = DateTimeOffset.Now;
            _startTime = DateTimeOffset.Now.TimeOfDay;

            Event = new Event
            {
                Duration = TimeSpan.FromHours(1),
                Recurrence = Event.RecurrenceType.None,
                BlockedAppInstallLocations = new List<string>(),
                BlockedSites = new Event.BlockedWebsites()
                {
                    BlockAllSites = false,
                    BlockedWebsiteUrls = new List<string>()
                }
            };

            UpdateStartDateTime();

            LoadApps();

            BlockedApplications.CollectionChanged += OnBlockedApplicationsChanged;
        }

        private void OnBlockedApplicationsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Event.BlockedAppInstallLocations = BlockedApplications.Select(app => app.InstallLocation).ToList();

            throw new NotImplementedException();
        }

        // Fills the list of all apps and the collection that will hold the filtered data
        private void LoadApps()
        {
            Apps = ApplicationRetriever.GetInstalledApplicationsFromRegistry();
            AppsFiltered = new ObservableCollection<ApplicationInfo>(Apps);
        }



        private Event _event;

        public Event Event
        {
            get { return _event; }
            set
            {
                if (_event != value)
                {
                    _event = value;
                    OnPropertyChanged(nameof(Event));
                }
            }
        }


        private DateTimeOffset _startDate;

        [Required]
        [StartDateValidation]
        public DateTimeOffset StartDate
        {
            get { return _startDate; }
            set
            {
                if(_startDate != value)
                {
                    SetProperty(ref _startDate, value, true);
                    UpdateStartDateTime();
                }
            }
        }

        private IEnumerable<ValidationResult> validationErrors;
        public IEnumerable<ValidationResult> ValidationErrors
        {
            get { return validationErrors; }
            set
            {
                if (validationErrors != value)
                {
                    SetProperty(ref validationErrors, value);
                }
            } 
        }

        private TimeSpan _startTime;
        public TimeSpan StartTime
        {
            get { return _startTime; }
            set
            {
                if (_startTime != value)
                {
                    _startTime = value;
                    UpdateStartDateTime();
                    OnPropertyChanged(nameof(StartTime));

                    ValidateProperty(StartTime, nameof(StartTime));
                }
            }
        }



        private int _recurrence;
        public int Recurrence
        {
            get { return _recurrence; }
            set {
                if (_recurrence != value)
                {
                    _recurrence = value;
                    UpdateRecurrence();
                    OnPropertyChanged(nameof(Recurrence));

                    ValidateProperty(Recurrence, nameof(Recurrence));
                }
            }
        }

        public class RecurrenceItem
        {
            public int Value { get; set; }
            public string Text { get; set; }
        }

        public IEnumerable<RecurrenceItem> RecurrenceItems
        {
            get { return GetRecurrenceItems(); }
        }


        private static IEnumerable<RecurrenceItem> GetRecurrenceItems()
        {
            yield return new RecurrenceItem() { Text = "None", Value = (int)Event.RecurrenceType.None };
            yield return new RecurrenceItem() { Text = "Daily", Value = (int)Event.RecurrenceType.Daily };
            yield return new RecurrenceItem() { Text = "Weekly", Value = (int)Event.RecurrenceType.Weekly };
            yield return new RecurrenceItem() { Text = "Monthly", Value = (int)Event.RecurrenceType.Monthly };
            yield return new RecurrenceItem() { Text = "Yearly", Value = (int)Event.RecurrenceType.Yearly };
        }




        // Property to bind BlockAllSites CheckBox
        public bool BlockAllSites
        {
            get => Event.BlockedSites.BlockAllSites;
            set
            {
                if (Event.BlockedSites.BlockAllSites != value)
                {
                    if (value)
                    {
                        Event.BlockedSites.BlockedWebsiteUrls = null;
                    }
                    else if (Event.BlockedSites.BlockedWebsiteUrls == null)
                    {
                        Event.BlockedSites.BlockedWebsiteUrls = new List<string>();
                    }

                    Event.BlockedSites.BlockAllSites = value;
                    OnPropertyChanged(nameof(BlockAllSites));
                    OnPropertyChanged(nameof(Event.BlockedSites.BlockedWebsiteUrls));
                }
            }
        }

        // Property to add new blocked website URL
        private string _newBlockedUrl;
        public string NewBlockedUrl
        {
            get => _newBlockedUrl;
            set => SetProperty(ref _newBlockedUrl, value);
        }

        // Command to add new blocked website URL
        public ICommand AddBlockedUrlCommand => new RelayCommand(AddBlockedUrl);

        private void AddBlockedUrl()
        {
            if (!string.IsNullOrWhiteSpace(NewBlockedUrl))
            {
                BlockAllSites = false;
                if (Event.BlockedSites.BlockedWebsiteUrls == null)
                {
                    Event.BlockedSites.BlockedWebsiteUrls = new List<string>();
                }

                Event.BlockedSites.BlockedWebsiteUrls.Add(NewBlockedUrl);
                NewBlockedUrl = string.Empty;
                OnPropertyChanged(nameof(Event.BlockedSites.BlockedWebsiteUrls));
            }
        }



        private void UpdateStartDateTime()
        {
            Event.Start = _startDate.Date + _startTime;
        }
        private void UpdateRecurrence()
        {
            Event.Recurrence = (Event.RecurrenceType)_recurrence;
        }

        private void UpdateBlockedApps()
        {
            //Event.BlockedAppInstallLocations = AppsFiltered.sel;
        }

        private void SetValidationErrors(object sender, DataErrorsChangedEventArgs e)
        {
            ValidationErrors = GetErrors(); 
        }
    }
}
