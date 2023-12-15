using System;
using System.Collections.Generic;

namespace RestrictRService.Models
{
    public partial class BlockedWebsiteUrl
    {
        public long EventId { get; set; }
        public string Url { get; set; } = null!;
    }
}
