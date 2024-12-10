namespace BUTTLYSS
{
    public enum InputMode { None = 0, Varied = 1, ContinuousRank = 2, Passthrough = 3 }

    public static class ButtlyssProperties
    {
        public static bool EmergencyStop = false;

        public static InputMode InputMode = InputMode.Varied;
        public static bool ForwardPatchedEvents => InputMode == InputMode.Varied;

        public static float CurrentSpeed;

        public static float StickForNormal => 2.0f;
        public static float SoftStickFor => 0.2f;

        public static float TapSpeed = 0.1f;

        public static float TimeSinceVibes;

        public static void ResetVibeTimes()
        {
            TimeSinceVibes = StickForNormal - SoftStickFor;
        }
    }
}