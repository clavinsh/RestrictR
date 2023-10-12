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

        // retrieves installed applications that can be found by looking at what
        // is stored in 'Microsoft\Windows\CurrentVersion\Uninstall' registry path
        // 
        // this method implements something similar to the powershell script 
        // by Jeff Hicks: https://petri.com/powershell-problem-solver-finding-installed-software-part-4/
        public static void GetInstalledApplicationsFromRegistry()
        {
            string registryPath = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall";

            RegistryKey key = Registry.LocalMachine.OpenSubKey(registryPath);

            if (key != null)
            {
                string[] subKeyNames = key.GetSubKeyNames();
                List<Dictionary<string, string>> resultList = new();

                
                // looking through each of the subkeys in the ...\Uninstall registry path as per the 'registryPath' variable
                foreach (string subKeyName in subKeyNames) 
                {
                    string keyPath = $"{registryPath}\\{subKeyName}";

                    RegistryKey subKey = key.OpenSubKey(subKeyName);

                    if (subKey != null)
                    {
                        // for clarity, the registry path to a sub key will be added
                        // to the dictionary for that particular key
                        Dictionary<string, string> valuesDict = new()
                        {
                            { "Path", keyPath}
                        };

                        string[] valueNames = subKey.GetValueNames();

                        // if some name matches any of the following literals (common for applications)
                        // its corresponding value will be added to the dict
                        foreach (string valueName in valueNames)
                        {
                            if (new[] { "Displayname", "DisplayVersion", "Publisher", "InstallDate", "InstallLocation", "Comments", "UninstallString" }.Contains(valueName))
                            {
                                string value = subKey.GetValue(valueName)?.ToString();
                                valuesDict[valueName] = value;
                            }
                        }

                        subKey.Close();

                        resultList.Add(valuesDict);
                    }
                }

                key.Close();


                foreach (var result in resultList)
                {
                    foreach (var pair in result) 
                    {
                        Debug.WriteLine($"{pair.Key}: {pair.Value}");
                    }
                    Debug.WriteLine("");
                }

            }
            else
            {
                Console.WriteLine("Registry path not found.");
            }

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
