namespace DataPacketLibrary
{
    [Serializable]
    public class ConfigurationPacket
    {
        public List<string>? BlockedAppInstallLocations { get; set; }

        public BlockedWebsites? BlockedSites { get; set; }

        public class BlockedWebsites
        {
            // BlockAllSites defines a 'header' for this data packet
            // false represents that only websites specified in BlockedWebsiteUrls should be handled
            // true represents that 'all of internet' should be blocked
            // a situation where BlockAllSites is true and BlockedWebsiteUrls is not null is an exception
            public bool BlockAllSites { get; set; }
            public List<string>? BlockedWebsiteUrls { get; set; }
        }
    }
}
