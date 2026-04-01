using BepInEx.Configuration;
using BepInEx;
using System;
using System.IO;
using ServerSync;
using UnityEngine;

namespace BreadcrumbTorch
{
    public enum TorchPlacement
    {
        Anywhere,
        OnlyDungeons,
        OnlyOutside
    }
    
    internal class ConfigurationFile
    {
        private static ConfigEntry<bool> _serverConfigLocked = null;
        
        public static ConfigEntry<bool> debug;
        public static ConfigEntry<KeyCode> torchSpawnKey;
        public static ConfigEntry<string> torchPieceName;
        public static ConfigEntry<TorchPlacement> torchPlacement;
        public static ConfigEntry<float> torchHeightOffset;
        public static ConfigEntry<bool> torchCharacterCollision;
        
        private static ConfigFile configFile;
        private static readonly string ConfigFileName = BreadcrumbTorch.GUID + ".cfg";
        private static readonly string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;

        private static readonly ConfigSync ConfigSync = new ConfigSync(BreadcrumbTorch.GUID)
        {
            DisplayName = BreadcrumbTorch.NAME,
            CurrentVersion = BreadcrumbTorch.VERSION,
            MinimumRequiredVersion = BreadcrumbTorch.VERSION
        };
        
        internal static void LoadConfig(BaseUnityPlugin plugin)
        {
            {
                configFile = plugin.Config;
                _serverConfigLocked = config("1 - General", "Lock Configuration", true, "If on, the configuration is locked and can be changed by server admins only.");
                _ = ConfigSync.AddLockingConfigEntry(_serverConfigLocked);
                debug = config("1 - General", "DebugMode", false, "Enabling/Disabling the debugging in the console (default = false)", false);
                torchSpawnKey = config("1 - General", "Spawn Key", KeyCode.U, "Key to spawn (default = U)", false);
                
                torchPieceName = config("2 - Spawn Options", "Prefab Name", "piece_groundtorch_green", "Piece to be spawned");
                torchPlacement = config("2 - Spawn Options", "Placement", TorchPlacement.Anywhere, "Placement where it is only allowed to spawn");
                torchHeightOffset = config("2 - Spawn Options", "Height Offset", 0.4f, "Small height correction to the spawned item on the ground");
                torchCharacterCollision = config("2 - Spawn Options", "Character Collision", true, "If on, characters will go through the spawned item without collision");
                SetupWatcher();
            }
        }

        private static void SetupWatcher()
        {
            FileSystemWatcher watcher = new FileSystemWatcher(Paths.ConfigPath, ConfigFileName);
            watcher.Changed += ReadConfigValues;
            watcher.Created += ReadConfigValues;
            watcher.Renamed += ReadConfigValues;
            watcher.IncludeSubdirectories = true;
            watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            watcher.EnableRaisingEvents = true;
        }

        private static void ReadConfigValues(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(ConfigFileFullPath)) return;
            try
            {
                Logger.Log("Attempting to reload configuration...");
                configFile.Reload();
                SettingsChanged(null, null);
                Logger.Log("Configuration reload complete.");
            }
            catch (Exception ex)
            {
                Logger.LogError($"There was an issue loading {ConfigFileName}: " + ex);
            }
        }

        private static void SettingsChanged(object sender, EventArgs e)
        {
            
        }

        private static ConfigEntry<T> config<T>(string group, string name, T value, string description,
            bool synchronizedSetting = true)
        {
            return config(group, name, value, new ConfigDescription(description), synchronizedSetting);
        }

        private static ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description,
            bool synchronizedSetting = true)
        {
            ConfigDescription extendedDescription =
                new ConfigDescription(
                    description.Description +
                    (synchronizedSetting ? " [Synced with Server]" : " [Not Synced with Server]"),
                    description.AcceptableValues, description.Tags);
            ConfigEntry<T> configEntry = configFile.Bind(group, name, value, extendedDescription);
            //var configEntry = Config.Bind(group, name, value, description);

            SyncedConfigEntry<T> syncedConfigEntry = ConfigSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

            return configEntry;
        }
    }
}