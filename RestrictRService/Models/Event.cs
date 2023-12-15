using System;
using System.Collections.Generic;

namespace RestrictRService.Models
{
    public partial class Event
    {
        public long EventId { get; set; }
        public byte[] Start { get; set; } = null!;
        public byte[] Duration { get; set; } = null!;
        public long Recurrence { get; set; }
    }
}
