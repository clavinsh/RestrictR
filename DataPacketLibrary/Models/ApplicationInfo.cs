namespace DataPacketLibrary.Models;

public partial class ApplicationInfo
{
    public int Id { get; set; }

    public int EventId { get; set; }

    public ApplicationInfo(string displayName, string displayVersion, string publisher, string installDate, string installLocation, string comments, string uninstallString, string registryPath)
    {
        DisplayName = displayName;
        DisplayVersion = displayVersion;
        Publisher = publisher;
        InstallDate = installDate;
        InstallLocation = installLocation;
        Comments = comments;
        UninstallString = uninstallString;
        RegistryPath = registryPath;
    }

    public string DisplayName { get; set; }
    public string DisplayVersion { get; set; }
    public string Publisher { get; set; }
    public string InstallDate { get; set; }
    public string InstallLocation { get; set; }
    public string Comments { get; set; }
    public string UninstallString { get; set; }
    public string RegistryPath { get; set; }
}
