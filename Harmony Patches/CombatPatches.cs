using HarmonyLib;
using MilkShake;
using UnityEngine;

namespace BUTTLYSS
{
    /// <summary>
    /// Patches camera shakes to trigger vibrate
    /// </summary>
    [HarmonyPatch(typeof(ShakeInstance), nameof(ShakeInstance.UpdateShake))]
    public static class CameraPatch
    {
        [HarmonyPostfix]
        static void UpdateShake(float deltaTime, ShakeInstance __instance) {
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
		public static void InitCast(string _skillName) {
            if (!Properties.ForwardPatchedEvents)
                return;
            // ButtplugManager.Vibrate(0.5f);
        }
    }

    /// <summary>
    /// Patches health loss to trigger vibration relative to loss amount
    /// </summary>
    [HarmonyPatch(typeof(StatusEntity), nameof(StatusEntity.Subtract_health))]
	public static class SubtractHealthPatch
	{
		[HarmonyPrefix]
		public static void SubtractHealth(StatusEntity __instance, int _value) {
            if (!Properties.ForwardPatchedEvents)
                return;
            ButtplugManager.VibrateRelative(_value , __instance._currentHealth);
        }
    }

    [HarmonyPatch(typeof(StatusEntity), nameof(StatusEntity.Change_Stamina))]
	public static class ChangeStaminaPatch
	{
		[HarmonyPrefix]
		public static void ChangeStamina(StatusEntity __instance, int _value) {
            if (!Properties.ForwardPatchedEvents)
                return;
            ButtplugManager.VibrateRelative(_value , __instance._currentStamina);
        }
    }

    [HarmonyPatch(typeof(StatusEntity), nameof(StatusEntity.Change_Mana))]
	public static class ChangeManaPatch
	{
		[HarmonyPrefix]
		public static void ChangeMana(StatusEntity __instance, int _value) {
            if (!Properties.ForwardPatchedEvents)
                return;
            ButtplugManager.VibrateRelative(_value , __instance._currentMana);
        }
    }
}