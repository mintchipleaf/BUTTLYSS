using System.Collections.Generic;
using System.Diagnostics;
using BepInEx.Logging;
using HarmonyLib;
using MilkShake;
using UnityEngine;
using UnityEngine.UI;

namespace BUTTLYSS
{
    /// <summary>
    /// Patches camera shakes to trigger vibrate
    /// </summary>
    [HarmonyPatch(typeof(ShakeInstance), nameof(ShakeInstance.UpdateShake))]
    public class CameraPatch
    {
        static void Postfix(float deltaTime, ShakeInstance __instance)
        {
            if (!Properties.ForwardPatchedEvents)
                return;
            ButtplugManager.Vibrate(Mathf.Clamp01(__instance.CurrentStrength / __instance.ShakeParameters.strength));
        }
    }

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
    [HarmonyPatch(typeof(PlayerMove), nameof(PlayerMove.Set_MovementAction))]
    public static class MovementPatch
    {
        static void Set_MovementAction_Postfix(MovementAction _mA) {
            if (!Properties.ForwardPatchedEvents)
                return;
            switch (_mA) {
                case MovementAction.JUMP:
                    ButtplugManager.Tap();
                    break;
                case MovementAction.DASH:
                    ButtplugManager.Tap();
                    break;
                case MovementAction.JUMPDASH:
                    ButtplugManager.Tap();
                    break;
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
    [HarmonyPatch(typeof(ChatBehaviour), nameof(ChatBehaviour.Send_ChatMessage))]
	public static class SendMessagePatch
	{
		[HarmonyPrefix]
		public static void Send_ChatMessage_Prefix(ChatBehaviour __instance, string _message) {
            ButtplugManager.Tap();
        }
    }


    /// <summary>
    /// Patches skill activation to trigger vibration
    /// </summary>
    [HarmonyPatch(typeof(PlayerCasting), nameof(PlayerCasting.Client_InitCastParams))]
	public static class SkillPatch
	{
		[HarmonyPrefix]
		public static void Postfix(string _skillName) {
            if (!Properties.ForwardPatchedEvents)
                return;
            ButtplugManager.Vibrate(0.5f);
        }
    }

    /// <summary>
    /// Patches health loss to trigger vibration relative to loss amount
    /// </summary>
    [HarmonyPatch(typeof(StatusEntity), nameof(StatusEntity.Subtract_health))]
	public static class SubtractHealthPatch
	{
		[HarmonyPrefix]
		public static void Subtract_health_Prefix(StatusEntity __instance, int _value) {
            if (!Properties.ForwardPatchedEvents)
                return;
            float relativeSpeed = Mathf.Max(Properties.TapSpeed, _value / __instance._currentHealth);
            ButtplugManager.Vibrate(relativeSpeed);
        }
    }
}