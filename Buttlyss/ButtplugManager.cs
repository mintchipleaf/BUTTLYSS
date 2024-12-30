using System;
using System.Security;
using System.Security.Permissions;
using BepInEx;
using HarmonyLib;
using UnityEngine;

#pragma warning disable CS0618

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace BUTTLYSS
{
    /// <summary>
    /// Manages overhead and state for harmony patching and buttplug use
    /// </summary>
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInProcess("ATLYSS.exe")]
    public class ButtplugManager : BaseUnityPlugin
    {
        /// <summary>
        /// Handler for Buttplug client functions
        /// </summary>
        private ButtplugClientHandler buttplugClientHandler;

        /// <summary>
        /// Time elapsed since last vibration command sent to server
        /// </summary>
        private float timeSinceVibeUpdate;

        /// <summary>
        /// Sets up method patches
        /// </summary>
        private void Awake() {
            buttplugClientHandler = new ButtplugClientHandler(Logger);

            new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll();

            Logger.LogInfo("BUTTLYSS Patches Loaded");
        }

        /// <summary>
        /// Connects to buttplug client
        /// </summary>
        private void Start() {
            // Load properties file and connect
            Properties.Load();
            buttplugClientHandler.TryRestartClient();
        }

        /// <summary>
        /// Handles sending vibration commands to buttplug client, and related local state
        /// </summary>
        private void Update() {
            State.MaxSpeedThisFrame = 0;

            State.VibeDuration += Time.unscaledDeltaTime;
            timeSinceVibeUpdate += Time.unscaledDeltaTime;

            if (buttplugClientHandler == null)
                return;

            if (Properties.EmergencyStop || Properties.InputMode == InputMode.None)
                State.CurrentSpeed = 0;

            // This shouldn't be run at more than 10hz, bluetooth can't keep up. Repeated commands will be
            // ignored in Buttplug, but quick updates can still cause lag.
            if (timeSinceVibeUpdate > 0.10) {
                buttplugClientHandler.VibrateAllDevices( Math.Min(State.CurrentSpeed * Properties.StrengthMultiplier, 1.0) );

                timeSinceVibeUpdate = 0;
            }

            // Reset to base speed if vibe time is above command length
            if (State.VibeDuration > Properties.MaxVibeDuration && Properties.InputMode == InputMode.Varied)
                State.CurrentSpeed = Properties.BaseSpeed;
        }

        /// <summary>
        /// Sets vibration speed
        /// </summary>
        /// <param name="speed">Speed of vibration command from 0 to 1</param>
        public static void Vibrate(float speed) {
            State.ResetVibeDuration();

            // Vibrate at the hightest current speed
            float newSpeed = Mathf.Clamp01(speed);
            newSpeed = Mathf.Max(newSpeed, State.MaxSpeedThisFrame);

            State.CurrentSpeed = newSpeed;
        }

        /// <summary>
        /// Vibrates an amount relative to the ratio between 'value' and 'relativeTo', but not below 'minSpeed'
        /// Amount = 'value' / 'relativeTo'
        /// </summary>
        /// <param name="value">Determines amount of vibration in ratio to 'relativeTo'</param>
        /// <param name="relativeTo">Amount that signifies 100% vibration relative to 'value'</param>
        /// <param name="minSpeed">Floor for vibration amount</param>
        /// <param name="scale">Value to scale the vibration by</param>
        public static void VibrateRelativeMin(float value, float relativeTo, float minSpeed, float scale = 1) {
            float relativeSpeed = Mathf.Max(minSpeed, value / relativeTo);
            Vibrate(relativeSpeed * scale);
        }

        /// <summary>
        /// Vibrates an amount relative to the ratio between 'value' and 'relativeTo'. Won't go lower than a Tap
        /// Amount = 'value' / 'relativeTo'
        /// </summary>
        /// <param name="value">Determines amount of vibration in ratio to 'relativeTo'</param>
        /// <param name="relativeTo">Amount that signifies 100% vibration relative to 'value'</param>
        /// <param name="scale">Value to scale the vibration by</param>
        public static void VibrateRelative(float value, float relativeToValue, float scale = 1) => VibrateRelativeMin(value, relativeToValue, Properties.TapSpeed, scale);

        /// <summary>
        /// Triggers a small vibration
        /// Used for very subtle inputs (menu button clicks, dashes, etc)
        /// </summary>
        public static void Tap() => Vibrate(Properties.TapSpeed);

        /// <summary>
        /// Disconnects from buttplug server
        /// </summary>
        private void OnDestroy() {
            buttplugClientHandler?.TryDisconnectClientSynchronous();
        }
    }
}