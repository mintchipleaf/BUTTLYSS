using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security;
using System.Security.Permissions;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using ButtplugManaged;

#pragma warning disable CS0618

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace BUTTLYSS
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.NAME, PluginInfo.VERSION)]
    public class ButtplugManager : BaseUnityPlugin
    {
        private ButtplugClient buttplugClient;
        private readonly List<ButtplugClientDevice> connectedDevices = new List<ButtplugClientDevice>();

        private float timeSinceVibeUpdate;

        private float strengthMultiplier => 0.8f;


        private void Awake() {
            Logger.LogInfo("BUTTLYSS Awake");
            var harmony = new Harmony("BUTTLYSS");
            harmony.PatchAll();
        }

        private void Start() {
            Task.Run(ReconnectClient);
        }

        private void Update() {
            // _timeSinceLastMove += Time.deltaTime;
            ButtlyssProperties.TimeSinceVibes += Time.deltaTime;
            timeSinceVibeUpdate += Time.deltaTime;

            if (buttplugClient == null)
                return;
            if (ButtlyssProperties.EmergencyStop)
                ButtlyssProperties.CurrentSpeed = 0;

            // This shouldn't be run at more than 10hz, bluetooth can't keep up. Repeated commands will be
            // ignored in Buttplug, but quick updates can still cause lag.
            if (timeSinceVibeUpdate > 0.10) {
                foreach (var buttplugClientDevice in connectedDevices) {
                    if (buttplugClientDevice.AllowedMessages.ContainsKey("VibrateCmd")) {
                        double vibeAmt = Math.Min(ButtlyssProperties.CurrentSpeed * strengthMultiplier, 1.0);
                        buttplugClientDevice.SendVibrateCmd(Math.Min(ButtlyssProperties.CurrentSpeed * strengthMultiplier, 1.0));
                        if(vibeAmt != 0)
                            Logger.LogInfo(vibeAmt);
                    }
                }

                timeSinceVibeUpdate = 0;
            }

            if (ButtlyssProperties.TimeSinceVibes > ButtlyssProperties.StickForNormal && ButtlyssProperties.InputMode == InputMode.Varied)
                ButtlyssProperties.CurrentSpeed = 0;
            else if (ButtlyssProperties.InputMode == InputMode.None)
                ButtlyssProperties.CurrentSpeed = 0;
        }

        public static void Vibrate(float originalAmount) {
            ButtlyssProperties.ResetVibeTimes();
            var amount = Mathf.Clamp(originalAmount, 0, 1);
            ButtlyssProperties.CurrentSpeed = amount;
        }

        // Used for very subtle vibrations (menu button clicks and dashes)
        public static void Tap(bool isMenu = false) {
            if (ButtlyssProperties.CurrentSpeed < ButtlyssProperties.TapSpeed) {
                Vibrate(ButtlyssProperties.TapSpeed);
            }
        }


        # region Buttplug Client

        public void TryRestartClient() {
            Logger.LogInfo("Restarting Buttplug client...");
            Task.Run(ReconnectClient);
        }

        private Uri GetConnectionUri() {
            return new Uri("ws://localhost:12345/buttplug");
        }

        private async Task TryKillClient() {
            if (buttplugClient == null)
                return;

            Logger.LogInfo("Disconnecting from Buttplug server...");
            buttplugClient.DeviceAdded -= AddDevice;
            buttplugClient.DeviceRemoved -= RemoveDevice;
            buttplugClient.ScanningFinished -= ScanningFinished;
            buttplugClient.ErrorReceived -= ErrorReceived;
            buttplugClient.ServerDisconnect -= ServerDisconnect;

            if (buttplugClient.IsScanning)
                await buttplugClient.StopScanningAsync();
            if (buttplugClient.Connected)
                await buttplugClient.DisconnectAsync();

            buttplugClient = null;
        }

        private async Task ReconnectClient() {
            var uri = GetConnectionUri();

            await TryKillClient();

            buttplugClient = new ButtplugClient("ATLYSS");
            buttplugClient.DeviceAdded += AddDevice;
            buttplugClient.DeviceRemoved += RemoveDevice;
            buttplugClient.ScanningFinished += ScanningFinished;
            buttplugClient.ErrorReceived += ErrorReceived;
            buttplugClient.ServerDisconnect += ServerDisconnect;

            Logger.LogInfo("Connecting to Buttplug server...");
            try {
                await buttplugClient.ConnectAsync(new ButtplugWebsocketConnectorOptions(uri));

                Task.Run(buttplugClient.StartScanningAsync);
            }
            catch (Exception ex) {
                Logger.LogError(ex.ToString());
            }
        }

        #endregion


        #region Device Callbacks

        private void AddDevice(object sender, DeviceAddedEventArgs args) {
            Logger.LogInfo("Device Added: " + args.Device.Name);
            connectedDevices.Add(args.Device);
        }

        private void RemoveDevice(object sender, DeviceRemovedEventArgs args) {
            Logger.LogInfo("Device Removed: " + args.Device.Name);
            connectedDevices.Remove(args.Device);
        }

        private void ScanningFinished(object sender, EventArgs args) {
            Logger.LogInfo("Scanning Finished");
        }

        private void ErrorReceived(object sender, ButtplugExceptionEventArgs args) {
            Logger.LogError("Error: " + args.Exception.Message);
        }

        private void ServerDisconnect(object sender, EventArgs args) {
            Logger.LogInfo("Server Disconnected");
            Task.Run(TryKillClient);
        }

        #endregion

        private void OnDestroy() {
            buttplugClient?.DisconnectAsync().Wait();
        }
    }
}