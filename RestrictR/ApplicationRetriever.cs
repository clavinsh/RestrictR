using Microsoft.Management.Infrastructure;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace RestrictR
{
    internal class ApplicationRetriever
    {
        const string UninstallPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";

        // retrieves installed applications that can be found by looking at what
        // is stored in 'Microsoft\Windows\CurrentVersion\Uninstall' registry path
        // 
        // this method implements something similar to the powershell script 
        // by Jeff Hicks: https://petri.com/powershell-problem-solver-finding-installed-software-part-4/
        public static List<ApplicationInfo> GetInstalledApplicationsFromRegistry()
        {
            List<ApplicationInfo> resultList = new();

            var registryView64 = RegistryView.Registry64;
            var registryView32 = RegistryView.Registry32;

            using var baseKey64 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, registryView64);
            using var baseKey32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, registryView32);

            var key64 = baseKey64.OpenSubKey(UninstallPath);
            var key32 = baseKey32.OpenSubKey(UninstallPath);

            if (key64 != null)
            {
                ProcessSubKeys(key64);
                key64.Close();
            }

            if (key32 != null)
            {
                ProcessSubKeys(key32);
                key32.Close();
            }

            return resultList;

            void ProcessSubKeys(RegistryKey key)
            {
                string[] possibleAppNames = key.GetSubKeyNames();

                foreach (string appName in possibleAppNames)
                {
                    using RegistryKey appKey = key.OpenSubKey(appName);

                    if (appKey != null)
                    {
                        Dictionary<string, string> valuesDict = new()
                        {
                            { "RegistryPath", appKey.ToString()}
                        };

                        valuesDict = GetAppInfo(appKey, valuesDict);

                        if (ValidAppInfo(valuesDict))
                        {
                            resultList.Add(new ApplicationInfo(valuesDict["DisplayName"],
                                valuesDict["DisplayVersion"], valuesDict["Publisher"],
                                valuesDict["InstallDate"], valuesDict["InstallLocation"],
                                valuesDict["Comments"], valuesDict["UninstallString"],
                                valuesDict["RegistryPath"]));
                        }
                    }
                }
            }
        }

        private static bool ValidAppInfo(Dictionary<string, string> valuesDict)
        {
            // at a minimum to even be able to block some app, will need the InstallLocation
            // to know what to block, and the DisplayName for UX
            string[] requiredKeys = { "DisplayName", "InstallLocation", };

            foreach (var key in requiredKeys)
            {
                if (!valuesDict.TryGetValue(key, out _))
                {
                    return false;
                }
            }

            // check for a valid install location
            if (!Path.IsPathFullyQualified(valuesDict["InstallLocation"]))
            {
                return false;
            }

            // display name should contain at least something for UX
            // otherwise what are we blocking, the average user won't be looking at 
            // the path of the app or other identification methods
            if (String.IsNullOrWhiteSpace(valuesDict["DisplayName"]))
            {
                return false;
            }

            return true;
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
