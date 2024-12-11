namespace BUTTLYSS
{
    /// <summary>
    /// Current state of buttplug-related values
    /// </summary>
    public static class State
    {
        /// <summary>
        /// Duration of the current vibration command
        /// </summary>
        public static float VibeDuration;

        /// <summary>
        /// Speed of the current vibration command
        /// </summary>
        public static float CurrentSpeed;

        /// <summary>
        /// Greatest speed amount for the current update frame
        /// </summary>
        public static float MaxSpeedThisFrame;


        /// <summary>
        /// Sets vibe duration to default
        /// </summary>
        public static void ResetVibeDuration()
        {
            VibeDuration = 0;
        }
    }
}