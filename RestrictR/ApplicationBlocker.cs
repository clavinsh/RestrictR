using System;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Windows.Media.Devices;

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


        // testing method for active process retrieval
        public static void GetProcessesTest() 
        {
            Process[] processes = Process.GetProcesses();

            foreach (Process process in processes) 
            {
                Debug.WriteLine($"id: {process.Id}, {process.ProcessName}");
            }

            

        }
    }
}
