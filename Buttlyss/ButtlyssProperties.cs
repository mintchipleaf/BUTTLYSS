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

        // Patch Control
        public bool VibrateOnUIButtons = true;
        public bool VibrateOnMoveActions = true;
        public bool VibrateOnChat = true;
        public bool VibrateOnItemAddRemove = true;
        public bool VibrateOnCameraShake = true;
        public bool VibrateOnHeal = true;
        public bool VibrateOnHurt = true;
        public bool VibrateOnStaminaUse = true;
        public bool VibrateOnDealDamage = true;

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
        /// Type of vibration inputs
        /// </summary>
        public static InputMode InputMode = InputMode.Varied;

        /// <summary>
        /// Whether event-specific vibrations should be sent to buttplug
        /// </summary>
        public static bool ForwardPatchedEvents => InputMode == InputMode.Varied && !EmergencyStop;

        /// <summary>
        /// Location of Intiface server including protocol, address, and port
        /// </summary>
        public static string ServerUrl = "ws://localhost:12345";

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
        public static float IdleSpeed = 0;

        /// <summary>
        /// Multiplier applied to screenshake-triggered vibrations
        /// </summary>
        public static float ScreenshakeMultiplier = 0.5f;


        #region Properties - Patch Control

        // General

        /// <summary>
        /// Whether to vibrate when UI buttons are pressed
        /// </summary>
        public static bool VibrateOnUIButtons = true;
        /// <summary>
        /// Whether to vibrate when movement actions are performed
        /// </summary>
        public static bool VibrateOnMoveActions = true;
        /// <summary>
        /// Whether to vibrate when chat messages are sent
        /// </summary>
        public static bool VibrateOnChat = true;

        // Interact

        /// <summary>
        /// Whether to vibrate when items are added or removed from the inventory
        /// </summary>
        public static bool VibrateOnItemAddRemove = true;

        // Combat

        /// <summary>
        /// Whether to vibrate when camera shakes occur
        /// </summary>
        public static bool VibrateOnCameraShake = true;
        /// <summary>
        /// Whether to vibrate when healing health
        /// </summary>
        public static bool VibrateOnHeal = true;
        /// <summary>
        /// Whether to vibrate when losing health
        /// </summary>
        public static bool VibrateOnHurt = true;
        /// <summary>
        /// Whether to vibrate when stamina is used
        /// </summary>
        public static bool VibrateOnStaminaUse = true;
        /// <summary>
        /// Whether to vibrate when dealing damage
        /// </summary>
        public static bool VibrateOnDealDamage = true;

        #endregion


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
            serializedProperties.BaseSpeed = IdleSpeed;
            serializedProperties.VibrateOnUIButtons = VibrateOnUIButtons;
            serializedProperties.VibrateOnMoveActions = VibrateOnMoveActions;
            serializedProperties.VibrateOnChat = VibrateOnChat;
            serializedProperties.VibrateOnItemAddRemove = VibrateOnItemAddRemove;
            serializedProperties.VibrateOnCameraShake = VibrateOnCameraShake;
            serializedProperties.VibrateOnHeal = VibrateOnHeal;
            serializedProperties.VibrateOnHurt = VibrateOnHurt;
            serializedProperties.VibrateOnStaminaUse = VibrateOnStaminaUse;
            serializedProperties.VibrateOnDealDamage = VibrateOnDealDamage;

            string contents = JsonUtility.ToJson(serializedProperties, prettyPrint: true);
            File.WriteAllText(settingsFilePath, contents);
        }

        /// <summary>
        /// Loads preferences from settings file
        /// </summary>
        public static void Load() {
            // Create folder and file as needed
            if(!File.Exists(settingsFilePath))
                Save();

            string json = File.ReadAllText(settingsFilePath);
            serializedProperties = JsonUtility.FromJson<SerializedProperties>(json);

            ServerUrl = serializedProperties.ServerUrl;
            MaxVibeDuration = serializedProperties.MaxVibeDuration;
            StrengthMultiplier = serializedProperties.StrengthMultiplier;
            TapSpeed = serializedProperties.TapSpeed;
            MinSpeed = serializedProperties.MinSpeed;
            IdleSpeed = serializedProperties.BaseSpeed;
            VibrateOnUIButtons = serializedProperties.VibrateOnUIButtons;
            VibrateOnMoveActions = serializedProperties.VibrateOnMoveActions;
            VibrateOnChat = serializedProperties.VibrateOnChat;
            VibrateOnItemAddRemove = serializedProperties.VibrateOnItemAddRemove;
            VibrateOnCameraShake = serializedProperties.VibrateOnCameraShake;
            VibrateOnHeal = serializedProperties.VibrateOnHeal;
            VibrateOnHurt = serializedProperties.VibrateOnHurt;
            VibrateOnStaminaUse = serializedProperties.VibrateOnStaminaUse;
            VibrateOnDealDamage = serializedProperties.VibrateOnDealDamage;

            Save();
        }

        #endregion

    }
}