#nullable disable
namespace RestrictRService.Models;

public partial class ApplicationInfo
{
    public int Id { get; set; }

    public int EventId { get; set; }

    public string DisplayName { get; }

    public string DisplayVersion { get; }

    public string Publisher { get; }

    public string InstallDate { get; }

    public string InstallLocation { get; }

    public string Comments { get; }

    public string UninstallString { get; }

    public string RegistryPath { get; }
}
