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
using ABI.Microsoft.UI.Xaml.Media.Animation;
using Windows.Devices.Geolocation;
using Windows.Data.Text;

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
        public static List<Dictionary<string, string>> GetInstalledApplicationsFromRegistry()
        {
            // have to explore both 32-bit and 64-bit paths
            string[] registryPaths = { "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall", "SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall" };

            List<Dictionary<string, string>> resultList = new();

            foreach (string registryPath in registryPaths) 
            {
                RegistryKey key = Registry.LocalMachine.OpenSubKey(registryPath) 
                    ?? throw new System.IO.IOException("Registry path not found.");

                string[] possibleAppNames = key.GetSubKeyNames();
               
                // looking through each of the subkeys in the ...\Uninstall registry path as per the 'registryPath' variable
                foreach (string appName in possibleAppNames) 
                {
                    //string keyPath = $"{registryPath}\\{subKeyName}";

                    RegistryKey appKey = key.OpenSubKey(appName);

                    if (appKey != null)
                    {
                        // for clarity, the registry path to a sub key will be added
                        // to the dictionary for that particular key
                        Dictionary<string, string> valuesDict = new()
                        {
                            { "Path", key.ToString()}
                        };

                        resultList.Add(GetAppInfo(appKey, valuesDict));
                    }
                }

                key.Close();
            }

            return resultList;
        }

        public static void GetUserSpecificApps()
        {

            throw new NotImplementedException();
            //RegistryKey key = Registry.Users;

            //string[] users = key.GetSubKeyNames();

            //List<Dictionary<string, string>> resultList = new();

            //if (key == null)
            //{
            //    throw new System.IO.IOException($"Registry path not found.");
            //}

            //foreach (string user in users)
            //{
            //    // filter out actual system users
            //    if (!user.EndsWith("_Classes", StringComparison.OrdinalIgnoreCase) && !user.Equals(".DEFAULT", StringComparison.OrdinalIgnoreCase))
            //    {
            //        string registryPath = $@"{user}\Software\Microsoft\Windows\CurrentVersion\Uninstall";
            //        RegistryKey userKey = key.OpenSubKey(registryPath);

            //        // data structure to store users - username, reg path, list of app values (dictionary)

            //        if(userKey != null) 
            //        {
            //            string[] possibleAppsNames = key.GetSubKeyNames();

            //            Dictionary<string, string> valuesDict = new()
            //            {
            //                {"Username", user},
            //                {"Path", registryPath}
            //            };


            //            foreach (string possibleAppName in possibleAppsNames) 
            //            {
            //                var appKey = userKey.OpenSubKey(possibleAppName);


                            
            //            }
            //        }

            //        userKey.Close();
            //        resultList.Add(valuesDict);
            //    }
            //}

            //key.Close();

            //foreach (var result in resultList)
            //{
            //    foreach (var pair in result)
            //    {
            //        Debug.WriteLine($"{pair.Key}: {pair.Value}");
            //    }
            //    Debug.WriteLine("");
            //}

            //throw new NotImplementedException();
        }

        public static List<Dictionary<string, string>> GetAppsFromWindowsInstaller()
        {
            using CimSession session = CimSession.Create(null); //localhost cim session

            if (!session.TestConnection())
            {
                throw new InvalidOperationException("Cannot establish connection with the WMI service.");
            }

            string query = "SELECT * FROM Win32_Product";

            IEnumerable<CimInstance> queryResults = session.QueryInstances("root\\cimv2", "WQL", query);

            List<Dictionary<string, string>> apps = new();

            foreach (CimInstance instance in queryResults)
            {
                Dictionary<string, string> appInfo = new()
                {
                    { "Caption", instance.CimInstanceProperties["Caption"].Value?.ToString() },
                    { "Description", instance.CimInstanceProperties["Description"].Value?.ToString() },
                    { "IdentifyingNumber", instance.CimInstanceProperties["IdentifyingNumber"].Value?.ToString() },
                    { "InstallLocation", instance.CimInstanceProperties["InstallLocation"].Value?.ToString() },
                    { "InstallState", instance.CimInstanceProperties["InstallState"].Value?.ToString() },
                    { "Name", instance.CimInstanceProperties["Name"].Value?.ToString() },
                    { "PackageCache", instance.CimInstanceProperties["PackageCache"].Value?.ToString() },
                    { "SKUNumber", instance.CimInstanceProperties["SKUNumber"].Value?.ToString() },
                    { "Vendor", instance.CimInstanceProperties["Vendor"].Value?.ToString() },
                    { "Version", instance.CimInstanceProperties["Version"].Value?.ToString() }
                };

                apps.Add(appInfo);
            }

            return apps;
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

        private static Dictionary<string, string> GetAppInfo(RegistryKey key, Dictionary<string, string> valuesDict)
        {
            if (key != null)
            {
                string[] valueNames = key.GetValueNames();
                var specifiedKeys = new[] { "DisplayName", "DisplayVersion", "Publisher", "InstallDate", "InstallLocation", "Comments", "UninstallString" };

                foreach (string specifiedKey in specifiedKeys)
                {
                    valuesDict[specifiedKey] = "";
                }

                // if some name matches any of the following literals (common for applications)
                // its corresponding value will be added to the dict
                foreach (string valueName in valueNames)
                {
                    if (specifiedKeys.Contains(valueName))
                    {
                        string value = key.GetValue(valueName)?.ToString() ?? "";
                        valuesDict[valueName] = value;
                    }
                }

                return valuesDict;
            }
            else throw new ArgumentNullException(nameof(key));
        }
    }
}
