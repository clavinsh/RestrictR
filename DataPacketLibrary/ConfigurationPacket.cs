using System.Security.Cryptography.X509Certificates;

namespace DataPacketLibrary
{
    [Serializable]
    public class ConfigurationPacket
    {
        public IEnumerable<Event> Events {get; set;} = new List<Event>();
    }

    public class Event
    {
        public Guid EventId { get; set; }

        public List<string>? BlockedAppInstallLocations { get; set; }

        public BlockedWebsites? BlockedSites { get; set; }

        public DateTime Start { get; set; }

        public TimeSpan Duration { get; set; }

        public RecurrenceType Recurrence { get; set; }

        public class BlockedWebsites
        {
            private bool _blockAllSites;
            private List<string>? _blockedWebsiteUrls;

            // BlockAllSites defines a 'header' for this data packet
            // false represents that only websites specified in BlockedWebsiteUrls should be handled
            // true represents that 'all of internet' should be blocked
            // a situation where BlockAllSites is true and BlockedWebsiteUrls is not null is an exception
            public bool BlockAllSites
            {
                get => _blockAllSites;
                set
                {
                    if (value && _blockedWebsiteUrls != null)
                    {
                        throw new InvalidOperationException("BlockedWebsiteUrls should be null when BlockedWebsiteUrls is true.");
                    }

                    _blockAllSites = value;
                }
            }
            public List<string>? BlockedWebsiteUrls
            {
                get => _blockedWebsiteUrls;
                set
                {
                    if (value != null && _blockAllSites)
                    {
                        throw new InvalidOperationException("BlockAllSites should be false when BlockedWebsiteUrls is not null.");
                    }

                    _blockedWebsiteUrls = value;
                }
            }
        }

        public enum RecurrenceType
        {
            None,
            Daily,
            Weekly,
            Monthly,
            Yearly
        }
    }
}
