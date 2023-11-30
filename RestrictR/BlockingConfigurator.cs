using DataPacketLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Principal;
using System.Text.Json;
using System.Threading.Tasks;
using static RestrictR.PipeCommunication;

namespace RestrictR
{
    internal class BlockingConfigurator
    {
        private List<string> BlockedApplications = new();
        private List<string> BlockedWebsites = new();

        public async Task SetBlockedApps(List<ApplicationInfo> apps)
        {
            foreach (ApplicationInfo app in apps)
            {
                if (!Path.IsPathFullyQualified(app.InstallLocation))
                {
                    throw new ArgumentException($"Invalid fully qualified path: {app.InstallLocation}");
                }
            }

            // If all paths are valid, clear and update the BlockedApplications list
            BlockedApplications.Clear();
            foreach (ApplicationInfo app in apps)
            {
                if (!BlockedApplications.Contains(app.InstallLocation))
                {
                    BlockedApplications.Add(app.InstallLocation);
                }
            }

            // serializes the blocked apps list and writes it to the common config file
            // used by the worker service


            //ConfigurationPacket data = new()
            //{
            //    BlockedAppInstallLocations = BlockedApplications,
            //    BlockedSites = new ConfigurationPacket.BlockedWebsites()
            //    { BlockAllSites = true, BlockedWebsiteUrls = null }
            //};

            Event data = new()
            {
                BlockedAppInstallLocations = BlockedApplications,
                BlockedSites = null
            };

            string configString = JsonSerializer.Serialize(data);
            await SendConfig(configString);

            //await ConfigWriter.WriteToCommonFolder(configString);
        }

        // method blocks all websites (effectively 'all of internet')
        public async Task SetBlockALLSites()
        {
            Event data = new()
            {
                BlockedAppInstallLocations = null,
                BlockedSites = new Event.BlockedWebsites() 
                {
                    BlockAllSites = true,
                    BlockedWebsiteUrls = null
                }
            };

            string configString = JsonSerializer.Serialize(data);

            await SendConfig(configString);
        }

        public async Task SetUnblockALLSites()
        {
            Event data = new()
            {
                BlockedAppInstallLocations = null,
                BlockedSites = new Event.BlockedWebsites()
                {
                    BlockAllSites = false,
                    BlockedWebsiteUrls = null
                }
            };

            string configString = JsonSerializer.Serialize(data);

            await SendConfig(configString);
        }


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


        public static bool IsUserAdmin()
        {
            WindowsIdentity currentIdentity = WindowsIdentity.GetCurrent();
            WindowsPrincipal currentPrincipal = new WindowsPrincipal(currentIdentity);

            // Check if the current user is a member of the Administrator group
            bool isAdmin = currentPrincipal.IsInRole(WindowsBuiltInRole.Administrator);

            return isAdmin;
        }
    }
}
