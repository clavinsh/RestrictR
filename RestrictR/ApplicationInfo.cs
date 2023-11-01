namespace RestrictR
{
    public class ApplicationInfo
    {
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

        public string DisplayName { get; }
        public string DisplayVersion { get; }
        public string Publisher { get; }
        public string InstallDate { get; }
        public string InstallLocation { get; }
        public string Comments { get; }
        public string UninstallString { get; }
        public string RegistryPath { get; }
    }
}
