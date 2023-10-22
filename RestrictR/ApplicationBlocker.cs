using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RestrictR
{
    internal class ApplicationBlocker
    {
        private List<string> BlockedApplications = new();

        public void AddBlockedApp(string installPath)
        {
            if (!Path.IsPathFullyQualified(installPath))
            {
                throw new ArgumentException("Invalid fully qualified path.");
            }

            if(BlockedApplications.Contains(installPath))
            {
                return;
            }

            BlockedApplications.Add(installPath);
        }

        public void ManageActiveProcesses()
        {
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

                        if (BlockedApplications.Contains(processPath, StringComparer.OrdinalIgnoreCase))
                        {
                            process.Kill();
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

        // From the ms docs: https://learn.microsoft.com/lv-lv/windows/win32/api/winbase/nf-winbase-queryfullprocessimagenamea?redirectedfrom=MSDN
        // Retrieves the full name of the executable image for the specified process.
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool QueryFullProcessImageName(IntPtr hProcess, uint dwFlags,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpExeName, 
            ref uint lpdwSize);

        //private const string RegistryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer\DisallowRun";

        //private static readonly string[] REGISTRY_APP_UNINSTALL_PATHS = { "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall", "SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall" };

        //const string Registry64BitAppUninstallPath = @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall";

        //public static void BlockApplication(string applicationName)
        //{
        //    if (applicationName == null)
        //    {
        //        throw new ArgumentNullException(nameof(applicationName),
        //            "Invalid application name specified.");
        //    }

        //    using RegistryKey key = Registry.LocalMachine.OpenSubKey(RegistryKeyPath)
        //        ?? throw new InvalidOperationException($"Registry key path: {RegistryKeyPath} was not found.");

        //    if (key.GetValueNames().Contains(applicationName))
        //    {
        //        Console.WriteLine($"{applicationName} is allready blocked in the registry.");
        //        return;
        //    }

        //    key.SetValue(applicationName, 1, RegistryValueKind.DWord);
        //    Console.WriteLine("Application blocked.");
        //    return;
        //}

        // Method sets up the Windows Registry keys
        // and values for application blocking
        public void RegistrySetup()
        {
            throw new NotImplementedException();
        }




    }
}
