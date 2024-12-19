using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Buttplug.Client;
using Buttplug.Core;
using BepInEx.Logging;

namespace BUTTLYSS
{
    /// <summary>
    /// Handles actions related to buttplug client operation
    /// </summary>
    public class ButtplugClientHandler
    {
        /// <summary>
        /// Singleton instance of ButtplugClientHandler
        /// </summary>
        public static ButtplugClientHandler Instance { get => instance; }
        private static ButtplugClientHandler instance;

        /// <summary>
        /// Active buttplug client
        /// </summary>
        private ButtplugClient buttplugClient;

        /// <summary>
        /// BepInEx console logger instance
        /// </summary>
        private ManualLogSource logger;

        /// <summary>
        /// Initializes singleton 
        /// </summary>
        /// <param name="logger">BepInEx logger from a BaseUnityPlugin object</param>
        public ButtplugClientHandler(ManualLogSource bepinexLogger) {
            instance = this;
            logger = bepinexLogger;
        }

        /// <summary>
        /// Sends vibration command to all devices capable of vibration
        /// </summary>
        /// <param name="speed">Speed to vibrate at, from 0 to 1</param>
        public void VibrateAllDevices(double speed) {
            foreach (ButtplugClientDevice device in buttplugClient.Devices) {
                if (device.VibrateAttributes.Count > 0) {
                    device.VibrateAsync(speed);

                    if(speed != 0)
                        logger.LogInfo(speed);
                }
            }
        }

        /// <summary>
        /// Imemdiately stops all device functions
        /// </summary>
        public void StopAllDevices() {
            foreach (ButtplugClientDevice device in buttplugClient.Devices) {
                device.Stop();
            }
        }


        #region Buttplug Client Operation

        /// <summary>
        /// Fully destroys and re-creates buttplug client, then connects it
        /// </summary>
        /// <returns>Restart Client task</returns>
        public Task TryRestartClient() {
            logger?.LogInfo("Restarting Buttplug client...");
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
        /// Disconnects buttplug client from server SYNCHRONOUSLY
        /// </summary>
        public void TryDisconnectClientSynchronous() {
            buttplugClient.DisconnectAsync().Wait();
        }

        /// <summary>
        /// Returns buttplug connector with currently set f of buttplug server
        /// </summary>
        /// <returns>Buttplug server URI</returns>
        private static ButtplugWebsocketConnector GetConnector() => new ButtplugWebsocketConnector(new Uri($"{Properties.ServerUrl}/buttplug"));

        /// <summary>
        /// Kills and recreates buttplug client, then tries to connect it to buttplug server
        /// </summary>
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
            logger?.LogInfo("Connecting to Buttplug server...");
            try {
                await buttplugClient.ConnectAsync( GetConnector() );
            }
            catch (ButtplugClientConnectorException ex) {
                logger?.LogError(
                    "Can't connect to Buttplug Server!" +
                    $"Message: {ex.InnerException.Message}");
            }
            catch (ButtplugHandshakeException ex) {
                logger?.LogError(
                    "Can't Handshake with Buttplug Server!" +
                    $"Message: {ex.InnerException.Message}");
            }
            catch (Exception ex) {
                logger?.LogError(
                    "Buttplug Server Error!" +
                    $"Message: {ex.InnerException.Message}");
            }

            // Scan for devices
            try {
                await buttplugClient.StartScanningAsync();
            }
            catch (ButtplugException ex) {
                logger?.LogError(
                    "Device Scanning failed!" +
                    $"Message: {ex.InnerException.Message}");
            }
        }

        /// <summary>
        /// Disconnect and reconnect the buttplug client to the server
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

            logger?.LogInfo("Disconnecting from Buttplug server...");
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
            logger?.LogInfo("Device Added: " + args.Device.Name);
        }

        /// <summary>
        /// Removes buttplug device from connected device list
        /// </summary>
        private void RemoveDevice(object sender, DeviceRemovedEventArgs args) {
            logger?.LogInfo("Device Removed: " + args.Device.Name);
        }

        /// <summary>
        /// Logs completion of device scan
        /// </summary>
        private void ScanningFinished(object sender, EventArgs args) {
            logger?.LogInfo("Scanning Finished");
        }

        /// <summary>
        /// Logs encountered errors
        /// </summary>
        private void ErrorReceived(object sender, ButtplugExceptionEventArgs args) {
            logger?.LogError($"Error: {args.Exception.Message}");
        }

        /// <summary>
        /// Kills buttplug client
        /// </summary>
        private void ServerDisconnect(object sender, EventArgs args) {
            logger?.LogInfo("Server Disconnected");
            Task.Run(TryKillClient);
        }

        #endregion
    }
}