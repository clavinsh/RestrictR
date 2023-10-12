using System;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Windows.Media.Devices;
using Microsoft.Management;
using Microsoft.Management.Infrastructure;

namespace RestrictR
{
    internal class ApplicationBlocker
    {
        private const string RegistryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer\DisallowRun";

        public static void BlockApplication(string applicationName)
        {
            if (applicationName == null) {
                throw new ArgumentNullException(nameof(applicationName),
                    "Invalid application name specified.");
            }

            using RegistryKey key = Registry.LocalMachine.OpenSubKey(RegistryKeyPath)
                ?? throw new InvalidOperationException($"Registry key path: {RegistryKeyPath} was not found.");

            if(key.GetValueNames().Contains(applicationName))
            {
                Console.WriteLine($"{applicationName} is allready blocked in the registry.");
                return;
            }

            key.SetValue(applicationName, 1, RegistryValueKind.DWord);
            Console.WriteLine("Application blocked.");
            return;
        }

        // Method sets up the Windows Registry keys
        // and values for application blocking
        public void RegistrySetup()
        {
            throw new NotImplementedException();
        }

        // retrieves installed applications that can be found
        // by looking what is stored in 'Microsoft\Windows\CurrentVersion\Uninstall'
        // registry path
        //
        // this method implements something similar to the powershell script 
        // by Jeff Hicks: https://petri.com/powershell-problem-solver-finding-installed-software-part-4/
        public static void GetInstalledApplicationsFromRegistry()
        {
            throw new NotImplementedException();
        }


        // testing method for active process retrieval
        public static void GetProcessesTest() 
        {
            using CimSession session = CimSession.Create(null);

            if (!session.TestConnection())
            {
                throw new InvalidOperationException("Cannot establish connection with the WMI service.");
            }

            string query = "SELECT ProcessId, Name, ExecutablePath FROM WIN32_Process";

            IEnumerable<CimInstance> queryResults = session.QueryInstances("root\\CIMv2", "WQL", query);

            foreach (CimInstance instance in queryResults)
            {
                int processId = Convert.ToInt32(instance.CimInstanceProperties["ProcessId"].Value);
                string name = instance.CimInstanceProperties["Name"].ToString();
                string path = instance.CimInstanceProperties["ExecutablePath"].ToString();

                Debug.WriteLine($"ProcessID: {processId}, Name: {name}, Path: {path}");
            }
        }
    }
}
