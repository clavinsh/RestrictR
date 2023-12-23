namespace RestrictRService
{
    public interface IApplicationBlocker
    {
        public void SetBlockedApps(List<string> appsInstallLocations);
        public void RemoveBlockedApps();
        public void ManageActiveProcesses();
    }
}
