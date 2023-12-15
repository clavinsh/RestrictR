using System;
using System.Collections.Generic;

namespace RestrictRService.Models
{
    public partial class BlockedApplication
    {
        public long EventId { get; set; }
        public string? DisplayName { get; set; }
        public string? DisplayVersion { get; set; }
        public string? Publisher { get; set; }
        public string? InstallDate { get; set; }
        public string InstallLocation { get; set; } = null!;
        public string? Comments { get; set; }
        public string? UninstallString { get; set; }
        public string? RegistryPath { get; set; }

        public virtual Event Event { get; set; } = null!;
    }
}
