using HarmonyLib;
using UnityEngine.UI;

namespace BUTTLYSS
{
    /// <summary>
    /// Patches menu buttons to trigger tap
    /// </summary>
    [HarmonyPatch(typeof(Button), "Press")]
    public static class ButtonPatch
    {
        static void Prefix() {
            if (!Properties.ForwardPatchedEvents)
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
        static void Postfix(MovementAction _mA) {
            if (!Properties.ForwardPatchedEvents)
                return;
            switch (_mA) {
                case MovementAction.JUMP:
                case MovementAction.DASH:
                case MovementAction.JUMPDASH:
                case MovementAction.LEDGEGRAB:
                    ButtplugManager.Tap();
                    break;
                default:
                    break;
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
		public static void Cmd_SendChatMessage_Prefix(string _message, ChatBehaviour.ChatChannel _chatChannel) {
            if (!Properties.ForwardPatchedEvents)
                return;
            ButtplugManager.Tap();
        }
    }
}