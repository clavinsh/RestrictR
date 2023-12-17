#nullable disable
namespace DataPacketLibrary.Models;

public partial class Event
{
    public int EventId { get; set; }
    public List<ApplicationInfo> BlockedApps { get; set; }
    public BlockedWebsites BlockedSites { get; set; }
    public DateTime Start { get; set; }
    public TimeSpan Duration { get; set; }
    public RecurrenceType Recurrence { get; set; }

    public enum RecurrenceType
    {
        None,
        Daily,
        Weekly,
        Monthly,
        Yearly
    }
}
