using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security;
using System.Security.Permissions;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using Buttplug.Client;
using Buttplug.Core;
using System.Linq;

#pragma warning disable CS0618

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace BUTTLYSS
{
    /// <summary>
    /// Handles actions related to buttplug client operation
    /// </summary>
    [BepInPlugin(PluginInfo.GUID, PluginInfo.NAME, PluginInfo.VERSION)]
    public class ButtplugManager : BaseUnityPlugin
    {
        /// <summary>
        /// Singleton instance of ButtplugManager
        /// </summary>
        public static ButtplugManager Instance {
            get {
                if (instance == null)
                    instance = new ButtplugManager();
                return instance;
            }
        }
        private static ButtplugManager instance;

        /// <summary>
        /// Active buttplug client
        /// </summary>
        private ButtplugClient buttplugClient;
        /// <summary>
        /// Time elapsed since last vibration command sent to server
        /// </summary>
        private float timeSinceVibeUpdate;


        /// <summary>
        /// Sets up method patches
        /// </summary>
        private void Awake() {
            instance = this;

            var harmony = new Harmony("BUTTLYSS");
            harmony.PatchAll();

            Logger.LogInfo("BUTTLYSS Patches Loaded");
        }

        /// <summary>
        /// Connects to buttplug client
        /// </summary>
        private void Start() {
            // Load properties file and connect
            Properties.Load();
            TryRestartClient();
        }

        /// <summary>
        /// Handles sending vibration commands to buttplug client, and related local state
        /// </summary>
        private void Update() {
            State.MaxSpeedThisFrame = 0;

            State.VibeDuration += Time.deltaTime;
            timeSinceVibeUpdate += Time.deltaTime;

            if (buttplugClient == null)
                return;

            if (Properties.EmergencyStop || Properties.InputMode == InputMode.None)
                State.CurrentSpeed = 0;

            // This shouldn't be run at more than 10hz, bluetooth can't keep up. Repeated commands will be
            // ignored in Buttplug, but quick updates can still cause lag.
            if (timeSinceVibeUpdate > 0.10) {
                foreach (ButtplugClientDevice device in buttplugClient.Devices) {
                    if (device.VibrateAttributes.Any()) {
                        double vibeAmt = Math.Min(State.CurrentSpeed * Properties.StrengthMultiplier, 1.0);
                        device.VibrateAsync(Math.Min(State.CurrentSpeed * Properties.StrengthMultiplier, 1.0));

                        if(vibeAmt != 0)
                            Logger.LogInfo(vibeAmt);
                    }
                }

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
            float newSpeed = Mathf.Clamp(speed, 0, 1);
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
        public static void VibrateRelativeMin(float value, float relativeTo, float minSpeed) {
            float relativeSpeed = Mathf.Max(minSpeed, value / relativeTo);
            Vibrate(relativeSpeed);
        }

        /// <summary>
        /// Vibrates an amount relative to the ratio between 'value' and 'relativeTo'
        /// Amount = 'value' / 'relativeTo'
        /// </summary>
        /// <param name="value">Determines amount of vibration in ratio to 'relativeTo'</param>
        /// <param name="relativeTo">Amount that signifies 100% vibration relative to 'value'</param>
        public static void VibrateRelative(float value, float relativeToValue) => VibrateRelativeMin(value, relativeToValue, Properties.TapSpeed);

        /// <summary>
        /// Triggers a small vibration
        /// Used for very subtle inputs (menu button clicks, dashes, etc)
        /// </summary>
        public static void Tap() => Vibrate(Properties.TapSpeed);


        #region Buttplug Client

        /// <summary>
        /// Fully destroys and re-creates buttplug client, then connects it
        /// </summary>
        /// <returns>Restart Client task</returns>
        public Task TryRestartClient() {
            Logger.LogInfo("Restarting Buttplug client...");
            return Task.Run(RestartClientAsync);
        }

        /// <summary>
        /// Connects buttplug client to server
        /// </summary>
        /// <returns>Connect Client task</returns>
        public Task TryReconnectClient() {
            return Task.Run(ReconnectClientAsync);
        }

        /// <summary>
        /// Returns buttplug connector with currently set f of buttplug server
        /// </summary>
        /// <returns>Buttplug server URI</returns>
        private static ButtplugWebsocketConnector GetConnector() => new ButtplugWebsocketConnector(new Uri($"{Properties.ServerUrl}/buttplug"));

        /// <summary>
        /// Kills and recreates buttplug client, then tries to connect it to buttplug server
        /// </summary>
        /// <returns></returns>
        private async Task RestartClientAsync() {
            await TryKillClient();

            buttplugClient = new ButtplugClient("ATLYSS");
            buttplugClient.DeviceAdded += AddDevice;
            buttplugClient.DeviceRemoved += RemoveDevice;
            buttplugClient.ScanningFinished += ScanningFinished;
            buttplugClient.ErrorReceived += ErrorReceived;
            buttplugClient.ServerDisconnect += ServerDisconnect;

            await ConnectClientAsync();
        }

        /// <summary>
        /// Attempts to connect existing buttplug client to buttplug server
        /// </summary>
        private async Task ConnectClientAsync() {
            // Connect to the server
            Logger.LogInfo("Connecting to Buttplug server...");
            try {
                await buttplugClient.ConnectAsync( GetConnector() );
            }
            catch (ButtplugClientConnectorException ex) {
                Logger.LogError(
                    "Can't connect to Buttplug Server!" +
                    $"Message: {ex.InnerException.Message}");
            }
            catch (ButtplugHandshakeException ex) {
                Logger.LogError(
                    "Can't Handshake with Buttplug Server!" +
                    $"Message: {ex.InnerException.Message}");
            }
            catch (Exception ex) {
                Logger.LogError(
                    "Buttplug Server Error!" +
                    $"Message: {ex.InnerException.Message}");
            }

            // Scan for devices
            try {
                await buttplugClient.StartScanningAsync();
            }
            catch (ButtplugException ex) {
                Logger.LogError(
                    "Device Scanning failed!" +
                    $"Message: {ex.InnerException.Message}");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private async Task ReconnectClientAsync() {
            if(buttplugClient.Connected)
                await buttplugClient.DisconnectAsync();

            await ConnectClientAsync();
        }

        /// <summary>
        /// Shuts down and stops buttplug client
        /// </summary>
        /// <returns>Async task for ending scans and disconnecting</returns>
        private async Task TryKillClient() {
            if (buttplugClient == null)
                return;

            Logger.LogInfo("Disconnecting from Buttplug server...");
            buttplugClient.DeviceAdded -= AddDevice;
            buttplugClient.DeviceRemoved -= RemoveDevice;
            buttplugClient.ScanningFinished -= ScanningFinished;
            buttplugClient.ErrorReceived -= ErrorReceived;
            buttplugClient.ServerDisconnect -= ServerDisconnect;

            await buttplugClient.StopScanningAsync();
            if (buttplugClient.Connected)
                await buttplugClient.DisconnectAsync();

            buttplugClient = null;
        }
        #endregion


        #region Device Callbacks

        /// <summary>
        /// Adds new buttplug device to connected device list
        /// </summary>
        private void AddDevice(object sender, DeviceAddedEventArgs args) {
            Logger.LogInfo("Device Added: " + args.Device.Name);
        }

        /// <summary>
        /// Removes buttplug device from connected device list
        /// </summary>
        private void RemoveDevice(object sender, DeviceRemovedEventArgs args) {
            Logger.LogInfo("Device Removed: " + args.Device.Name);
        }

        /// <summary>
        /// Logs completion of device scan
        /// </summary>
        private void ScanningFinished(object sender, EventArgs args) {
            Logger.LogInfo("Scanning Finished");
        }

        /// <summary>
        /// Logs encountered errors
        /// </summary>
        private void ErrorReceived(object sender, ButtplugExceptionEventArgs args) {
            Logger.LogError($"Error: {args.Exception.Message}");
        }

        /// <summary>
        /// Kills buttplug client
        /// </summary>
        private void ServerDisconnect(object sender, EventArgs args) {
            Logger.LogInfo("Server Disconnected");
            Task.Run(TryKillClient);
        }

        #endregion

        /// <summary>
        /// Disconnects from buttplug server
        /// </summary>
        private void OnDestroy() {
            buttplugClient?.DisconnectAsync().Wait();
        }
    }
}