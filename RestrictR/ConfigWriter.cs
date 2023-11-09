using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Management.Core;
using Windows.Storage;

namespace RestrictR
{
    internal class ConfigWriter
    {
        const string AppName = "RestrictR";
        const string ConfigFileName = "myconfig.json";

        // writes the json string to the common configuration folder
        // this file and the config will be used by the worker service
        public static async Task WriteToCommonFolder(string config)
        {
            // DOES NOT WORK, WILL NEED FIXING!!!!
            var commonAppDataFolder = ApplicationDataManager.CreateForPackageFamily(Package.Current.Id.FamilyName).GetPublisherCacheFolder(AppName);

            var configFile = await commonAppDataFolder.CreateFileAsync(ConfigFileName, CreationCollisionOption.OpenIfExists);

            await FileIO.WriteTextAsync(configFile, config);
        }
    }
}
