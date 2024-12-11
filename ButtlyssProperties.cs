namespace BUTTLYSS
{
    /// <summary>
    /// Types of vibration inputs
    /// </summary>
    public enum InputMode { None = 0, Varied = 1, Passthrough = 2 }

    /// <summary>
    /// User-changable static properties related to buttplug operation
    /// </summary>
    public static class Properties
    {
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
        public static bool ForwardPatchedEvents => InputMode == InputMode.Varied;

        /// <summary>
        /// Longest duration for a single vibration command
        /// </summary>
        public static float MaxVibeCommandLength => 0.2f;

        /// <summary>
        /// Speed of all tap commands
        /// </summary>
        public static float TapSpeed = 0.1f;
    }
}