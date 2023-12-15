using System;
using System.Collections.Generic;

namespace RestrictRService.Models
{
    public partial class BlockedWebsite
    {
        public long EventId { get; set; }
        public byte[] BlockAllSites { get; set; } = null!;

        public virtual Event Event { get; set; } = null!;
    }
}
