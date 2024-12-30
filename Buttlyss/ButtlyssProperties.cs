using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BUTTLYSS
{
    /// <summary>
    /// Types of vibration inputs
    /// </summary>
    public enum InputMode { None = 0, Varied = 1, Passthrough = 2 }

    /// <summary>
    /// Set of properties that is serialized out to file
    /// </summary>
    public class SerializedProperties {
        public string ServerUrl;
        public float MaxVibeDuration;
        public float StrengthMultiplier;
        public float TapSpeed;
        public float MinSpeed;
        public float BaseSpeed;
    }

    /// <summary>
    /// User-changable static properties related to buttplug operation
    /// </summary>
    public static class Properties
    {
        // Seriliazation Properties
        private static SerializedProperties serializedProperties = new SerializedProperties();

        private const string SETTINGS_FOLDER_NAME = "/profileCollections/";
        private const string SETTINGS_FILE_NAME = "buttlyssSettings.json";

        private static string settingsFolderPath = Application.dataPath + SETTINGS_FOLDER_NAME;
        private static string settingsFilePath = settingsFolderPath + SETTINGS_FILE_NAME;

        /// <summary>
        /// List of available commands' names
        /// </summary>
        public static Dictionary<string,string> CommandInfo = new Dictionary<string, string> {
            {"Stop", "Immediately stops all vibration"},
            {"Start", "Starts vibrations again after using 'Stop'"},
            {"Reload", "Updates preferences from settings file"},
            {"Reconnect", "Attempts to reconnect to server URL in settings"}
        };

        /// <summary>
        /// List of available properties' names
        /// </summary>
        public static Dictionary<string,string> PropertyInfo = new Dictionary<string, string> {
            {"IntifaceServer", "URL and port of the intiface server"},
            {"MaxVibeDuration", "Max length of time for individual vibrations"},
            {"StrengthMultiplier", "Multiplier applied to all vibrations"},
            {"TapSpeed", "Vibration speed for small taps"},
            {"MinSpeed", "Lowest possible vibration speed"},
            {"BaseSpeed", "Speed to vibrate when idle, (no vibration events occuring)"}
        };

        /// <summary>
        /// Immediately stops all current and future vibrations
        /// </summary>
        public static bool EmergencyStop = false;

        /// <summary>
        /// Location of Intiface server including protocol, address, and port
        /// </summary>
        public static string ServerUrl = "ws://192.168.1.150:12345";

        /// <summary>
        /// Type of vibration inputs
        /// </summary>
        public static InputMode InputMode = InputMode.Varied;
        /// <summary>
        /// Whether event-specific vibrations should be sent to buttplug
        /// </summary>
        public static bool ForwardPatchedEvents => InputMode == InputMode.Varied && !EmergencyStop;

        /// <summary>
        /// Longest duration for a single vibration command
        /// </summary>
        public static float MaxVibeDuration = 0.2f;

        /// <summary>
        /// Multiplier applied to all vibration speeds
        /// </summary>
        public static float StrengthMultiplier = 0.8f;

        /// <summary>
        /// Speed of all tap commands
        /// </summary>
        public static float TapSpeed = 0.1f;

        /// <summary>
        /// Slowest vibration speed
        /// </summary>
        public static float MinSpeed = 0.05f;

        /// <summary>
        /// Speed when not actively vibrating
        /// </summary>
        public static float BaseSpeed = 0;

        /// <summary>
        /// Multiplier applied to screenshake-triggered vibrations
        /// </summary>
        public static float ScreenshakeMultiplier = 0.5f;

        #region I/O

        /// <summary>
        /// Saves out preferences to settings file
        /// </summary>
        public static void Save() {
            serializedProperties.ServerUrl = ServerUrl;
            serializedProperties.MaxVibeDuration = MaxVibeDuration;
            serializedProperties.StrengthMultiplier = StrengthMultiplier;
            serializedProperties.TapSpeed = TapSpeed;
            serializedProperties.MinSpeed = MinSpeed;
            serializedProperties.BaseSpeed = BaseSpeed;

            string contents = JsonUtility.ToJson(serializedProperties, prettyPrint: true);
            File.WriteAllText(settingsFilePath, contents);
        }

        /// <summary>
        /// Loads preferences from settings file
        /// </summary>
        public static void Load() {
            // Create folder and file as needed
            // if(!Directory.Exists(settingsFolderPath))
            //     Directory.CreateDirectory(settingsFolderPath);
            if(!File.Exists(settingsFilePath))
                Save();

            string json = File.ReadAllText(settingsFilePath);
            serializedProperties = JsonUtility.FromJson<SerializedProperties>(json);

            ServerUrl = serializedProperties.ServerUrl;
            MaxVibeDuration = serializedProperties.MaxVibeDuration;
            StrengthMultiplier = serializedProperties.StrengthMultiplier;
            TapSpeed = serializedProperties.TapSpeed;
            MinSpeed = serializedProperties.MinSpeed;
            BaseSpeed = serializedProperties.BaseSpeed;
        }

        #endregion

    }
}