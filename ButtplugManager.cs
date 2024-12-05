using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BepInEx;
using ButtplugManaged;
using UnityEngine;

namespace BUTTLYSS
{
    public enum InputMode { None = 0, Varied = 1, ContinuousRank = 2, Passthrough = 3 }

    [BepInPlugin(PluginInfo.GUID, PluginInfo.NAME, PluginInfo.VERSION)]
    public class ButtplugManager : BaseUnityPlugin
    {

        public static ButtplugManager Instance;
        public static readonly plog.Logger Log = new plog.Logger("BUTTLYSS");
        public bool emergencyStop = false;

        private ButtplugClient buttplugClient;
        private readonly List<ButtplugClientDevice> connectedDevices = new List<ButtplugClientDevice>();

        public float currentSpeed = 0;
        public float currentRank = 0;
        public float currentLinearTime = 0;

        private float _timeSinceVibes;
        private float _timeSinceVibeUpdate;

        // Guh guh
        private readonly Queue<string> _logQueue = new Queue<string>();
        private readonly Queue<string> _errorLogQueue = new Queue<string>();

        private float TimeSinceVibes => _timeSinceVibes;

        private float StickForNormal => 2.0f;
        private float SoftStickFor => 0.2f;
        private float StrengthMultiplier => 0.8f;
        public InputMode InputMode => InputMode.Varied;
        public static bool ForwardPatchedEvents => Instance != null && Instance.InputMode == InputMode.Varied;

        private float LinearPosMin => 0.1f;
        private float LinearPosMax => 0.9f;
        private float LinearTimeMin => 0.3f;
        private float LinearTimeMax => 1.5f;

        private bool StrokeWhileIdle => false;

        // Toggle for movement direction state
        private bool _moveMax = true;

        // Current setting for stroke time, recalculated based on rank at the end of each stroke
        private float _moveTime;

        // Timer for calculating stroke command frequency
        private float _timeSinceLastMove;

        private void Awake()
        {
            gameObject.hideFlags = HideFlags.HideAndDontSave;
        }

        private void Start()
        {
            Log.Info("Initializing BUTTLYSS");
            Instance = this;
            // Start at the slowest Linear time.
            _moveTime = LinearTimeMax;

            Log.Info("Patching Game...");
            var harmony = HarmonyLib.Harmony.CreateAndPatchAll(typeof(MovementPatch));
            harmony.PatchAll(typeof(ButtonPatch)); // Intercepts UI Button presses

            // Connect the buttplug.io client
            Task.Run(ReconnectClient);
        }

        public void TryRestartClient()
        {
            Log.Info("Restarting Buttplug client...");
            Task.Run(ReconnectClient);
        }

        private Uri GetConnectionUri()
        {
            return new Uri("ws://localhost:12345/buttplug");
        }

        private async Task TryKillClient()
        {
            if (buttplugClient == null) return;
            _logQueue.Enqueue("Disconnecting from Buttplug server...");
            buttplugClient.DeviceAdded -= AddDevice;
            buttplugClient.DeviceRemoved -= RemoveDevice;
            buttplugClient.ScanningFinished -= ScanningFinished;
            buttplugClient.ErrorReceived -= ErrorReceived;
            buttplugClient.ServerDisconnect -= ServerDisconnect;

            if (buttplugClient.IsScanning) await buttplugClient.StopScanningAsync();
            if (buttplugClient.Connected) await buttplugClient.DisconnectAsync();

            buttplugClient = null;
        }

        private async Task ReconnectClient()
        {
            var uri = GetConnectionUri();

            await TryKillClient();

            buttplugClient = new ButtplugClient("ATLYSS");
            buttplugClient.DeviceAdded += AddDevice;
            buttplugClient.DeviceRemoved += RemoveDevice;
            buttplugClient.ScanningFinished += ScanningFinished;
            buttplugClient.ErrorReceived += ErrorReceived;
            buttplugClient.ServerDisconnect += ServerDisconnect;

            _logQueue.Enqueue("Connecting to Buttplug server...");
            try
            {
                await buttplugClient.ConnectAsync(new ButtplugWebsocketConnectorOptions(uri));

                Task.Run(buttplugClient.StartScanningAsync);
            }
            catch (Exception ex)
            {
                _errorLogQueue.Enqueue(ex.ToString());
            }
        }

        public static void Vibrate(float originalAmount)
        {
            if (!Instance || Instance.emergencyStop) return;
            ResetVibeTimes();
            var amount = Mathf.Clamp(originalAmount, 0, 1);
            Instance.currentSpeed = amount;
        }

        // Used for very subtle vibrations (menu button clicks and dashes)
        public static void Tap(bool isMenu = false)
        {
            if (!Instance || Instance.emergencyStop) return;
            ResetVibeTimes();
            if (Instance.currentSpeed < 0.1f)
            {
                Instance.currentSpeed = 0.1f;
            }
        }

        private void Update()
        {
            _timeSinceLastMove += Time.deltaTime;
            _timeSinceVibes += Time.deltaTime;
            _timeSinceVibeUpdate += Time.deltaTime;

            if (_logQueue.Count > 0 || _errorLogQueue.Count > 0)
            {
                while (_logQueue.Count > 0)
                {
                    var message = _logQueue.Dequeue();
                    Log.Info(message);
                }

                while (_errorLogQueue.Count > 0)
                {
                    var message = _errorLogQueue.Dequeue();
                    Log.Error(message);
                }
            }

            if (buttplugClient == null) return;
            if (emergencyStop) currentSpeed = 0;


            // This shouldn't be run at more than 10hz, bluetooth can't keep up. Repeated commands will be
            // ignored in Buttplug, but quick updates can still cause lag.
            if (_timeSinceVibeUpdate > 0.10)
            {
                foreach (var buttplugClientDevice in connectedDevices)
                {
                    if (buttplugClientDevice.AllowedMessages.ContainsKey("VibrateCmd"))
                    {
                        buttplugClientDevice.SendVibrateCmd(Math.Min(currentSpeed * StrengthMultiplier, 1.0));
                    }
                    // Only trigger stroker movement if we're using continuous rank mode. Variable mode doesn't make sense for this.
                    if (InputMode == InputMode.ContinuousRank && (StrokeWhileIdle || currentSpeed > 0.00001)) {
                        if (buttplugClientDevice.AllowedMessages.ContainsKey("LinearCmd"))
                        {
                            if (_timeSinceLastMove > _moveTime)
                            {
                                _moveTime = Math.Max(LinearTimeMax - ((LinearTimeMax - LinearTimeMin) * (currentSpeed * StrengthMultiplier)), LinearTimeMin);
                                buttplugClientDevice.SendLinearCmd((uint)(1000 * _moveTime), _moveMax ? LinearPosMin : LinearPosMax);
                            }
                        }
                    }
                    else
                    {
                        // This resets our move time so that once MoveWhileIdle or speed goes > 1, we'll be sure to actually trigger a command.
                        _moveTime = 0;
                    }
                }
                // On the extremely rare chance someone has multiple linear devices, don't reset values
                // until after commands have been sent to all of them.
                //
                // Also, don't run this if we're not stroking already.
                if (_moveTime > 0 && _timeSinceLastMove > _moveTime)
                {
                    _timeSinceLastMove = 0;
                    _moveMax = !_moveMax;
                }
                _timeSinceVibeUpdate = 0;
            }

            if (TimeSinceVibes > StickForNormal && InputMode == InputMode.Varied) currentSpeed = 0;
            else if (InputMode == InputMode.None) currentSpeed = 0;
        }

        private static void ResetVibeTimes()
        {
            Instance._timeSinceVibes = Instance.StickForNormal - Instance.SoftStickFor;
        }

        private void AddDevice(object sender, DeviceAddedEventArgs args)
        {
            _logQueue.Enqueue("Device Added: " + args.Device.Name);
            connectedDevices.Add(args.Device);
        }

        private void RemoveDevice(object sender, DeviceRemovedEventArgs args)
        {
            _logQueue.Enqueue("Device Removed: " + args.Device.Name);
            connectedDevices.Remove(args.Device);
        }

        private void ScanningFinished(object sender, EventArgs args)
        {
            _logQueue.Enqueue("Scanning Finished");
        }

        private void ErrorReceived(object sender, ButtplugExceptionEventArgs args)
        {
            _errorLogQueue.Enqueue("Error: " + args.Exception.Message);
        }

        private void ServerDisconnect(object sender, EventArgs args)
        {
            _logQueue.Enqueue("Server Disconnected");
            Task.Run(TryKillClient);
        }

        private void OnDestroy()
        {
            buttplugClient?.DisconnectAsync().Wait();
        }
    }
}
