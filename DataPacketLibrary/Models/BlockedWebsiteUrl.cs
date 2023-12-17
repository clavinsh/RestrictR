#nullable disable
namespace DataPacketLibrary.Models;

public class BlockedWebsiteUrl
{
    public int Id { get; set; }

    public int BlockedWebsiteId { get; set; }

    public string Url { get; set; }
}
