using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace RestrictRService
{
    // ApplicationBlocker class deals with blocking (killing active processes)
    // applications and providing methods to set which apps need to be blocked
    public class ApplicationBlocker : IApplicationBlocker
    {
        private List<string> BlockedApplicationsInstallLocations = new();

        // time in milliseconds for which the service waits for the process to close normally,
        // before killing it by force
        private int gracefulExitTimeout = 5000; 

        // Retrieves the full name of the executable image for the specified process.
        // From the ms docs: https://learn.microsoft.com/lv-lv/windows/win32/api/winbase/nf-winbase-queryfullprocessimagenamea?redirectedfrom=MSDN
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool QueryFullProcessImageName(IntPtr hProcess, uint dwFlags,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpExeName,
            ref uint lpdwSize);

        public void SetBlockedApps(List<string> appsInstallLocations)
        {
            foreach (string app in appsInstallLocations)
            {
                if (!Path.IsPathFullyQualified(app))
                {
                    throw new ArgumentException($"Invalid fully qualified path: {app}");
                }
            }

            // If all paths are valid, clear and update the BlockedApplications list
            BlockedApplicationsInstallLocations.Clear();

            foreach (string app in appsInstallLocations)
            {
                if (!BlockedApplicationsInstallLocations.Contains(app))
                {
                    BlockedApplicationsInstallLocations.Add(app);
                }
            }
        }

        public void RemoveBlockedApps()
        {
            BlockedApplicationsInstallLocations.Clear();
        }

        public async void ManageActiveProcesses()
        {
            // No need to evaluate processes if there will be nothing to block
            if (BlockedApplicationsInstallLocations.Count == 0)
            { return; }

            var processes = Process.GetProcesses();

            foreach (var process in processes)
            {
                try
                {
                    StringBuilder processPathBuilder = new StringBuilder(1024);

                    uint bufferSize = (uint)processPathBuilder.Capacity;

                    if (QueryFullProcessImageName(process.Handle, 0, processPathBuilder, ref bufferSize))
                    {
                        string processPath = processPathBuilder.ToString();
                        string fullProcessPath = Path.GetFullPath(processPath);


                        foreach (var blockedAppInstallLocation in BlockedApplicationsInstallLocations)
                        {
                            string fullInstallLocationPath = Path.GetFullPath(blockedAppInstallLocation);

                            if (fullProcessPath.StartsWith(fullInstallLocationPath))
                            {
                                await TryKillProcessAsync(process);
                            }
                        }
                    }
                    else
                    {
                        Debug.WriteLine($"Unable to retrieve process path for process with ID {process.Id}");
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"Error occured while killing a blocked process: {e.Message}");
                }
            }
        }

        private async Task TryKillProcessAsync(Process process)
        {
            await Task.Run(() =>
            {
                // before bombarding the process constantly with closes and kills,
                // check if it already has exited (can/will happen if the process takes some time to close,
                // and we have reached the next loop of the main manage method (1 second delay)
                if (!process.HasExited)
                {
                    bool hasExited = process.CloseMainWindow();

                    if (!hasExited)
                    {
                        hasExited = process.WaitForExit(gracefulExitTimeout);
                    }

                    if (!hasExited)
                    {
                        process.Kill();
                    }
                }
            });
        }
    }
}
