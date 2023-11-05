using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RestrictRService
{
    // ApplicationBlocker class deals with blocking (killing active processes)
    // applications and providing methods to set which apps need to be blocked
    public class ApplicationBlocker
    {
        private List<string> BlockedApplicationsInstallLocations = new();

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

        public void ManageActiveProcesses()
        {
            // No need to evaluate processes if there will be nothing to block
            if(BlockedApplicationsInstallLocations.Count == 0)
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
                                process.Kill();
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


        // Method sets up the Windows Registry keys
        // and values for application blocking
        public void RegistrySetup()
        {
            throw new NotImplementedException();
        }
    }
}
