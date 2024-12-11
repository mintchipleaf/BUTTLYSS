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