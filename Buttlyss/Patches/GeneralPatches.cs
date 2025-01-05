using HarmonyLib;
using UnityEngine.UI;

namespace BUTTLYSS.Patches
{
    /// <summary>
    /// Patches menu buttons to trigger tap
    /// </summary>
    [HarmonyPatch(typeof(Button), "Press")]
    public static class ButtonPatch
    {
        [HarmonyPrefix]
        public static void Press() {
            if (!Properties.ForwardPatchedEvents || !Properties.VibrateOnUIButtons)
                return;

            ButtplugManager.Tap();
        }
    }

    /// <summary>
    /// Patches various movement to trigger taps
    /// </summary>
    [HarmonyPatch(typeof(PlayerMove), "Set_MovementAction")]
    public static class MovementPatch
    {
        [HarmonyPostfix]
        public static void SetMovementAction(MovementAction _mA) {
            if (!Properties.ForwardPatchedEvents || !Properties.VibrateOnMoveActions)
                return;

            switch (_mA) {
                case MovementAction.JUMP:
                case MovementAction.DASH:
                case MovementAction.JUMPDASH:
                case MovementAction.LEDGEGRAB:
                    ButtplugManager.Tap();
                    break;
                default: break;
            }

            return;
        }
    }

    /// <summary>
    /// Patches chat messages being sent to trigger tap
    /// </summary>
    [HarmonyPatch(typeof(ChatBehaviour), nameof(ChatBehaviour.Cmd_SendChatMessage))]
	public static class CmdSendMessagePatch
	{
		[HarmonyPrefix]
		public static bool SendChatMessage(string _message, ChatBehaviour.ChatChannel _chatChannel, ChatBehaviour __instance) {
            // Check for commands first, and ignore all subsequent chat handling
            if(ButtlyssConsole.TryHandleChatCommands(_message, __instance))
                return false;

            // It's something else so just tap
            if (!Properties.ForwardPatchedEvents || !Properties.VibrateOnChat)
                return true;

            ButtplugManager.Tap();
            return true;
        }

    }
}