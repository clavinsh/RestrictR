using System;
using System.Collections.Generic;

namespace DataPacketLibrary.Models;

public class BlockedWebsites
{
    public int Id { get; set; }

    public int EventId { get; set; }

    public bool BlockAllSites { get; set; }

    public List<BlockedWebsiteUrl> BlockedWebsiteUrls { get; set; } = new List<BlockedWebsiteUrl>();
}
