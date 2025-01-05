using HarmonyLib;
using MilkShake;
using UnityEngine;

namespace BUTTLYSS.Patches
{
    /// <summary>
    /// Patches camera shakes to trigger vibrate
    /// </summary>
    [HarmonyPatch(typeof(ShakeInstance), nameof(ShakeInstance.UpdateShake))]
    public static class CameraPatch
    {
        [HarmonyPostfix]
        static void UpdateShake(ShakeInstance __instance, float deltaTime) {
            if (!Properties.ForwardPatchedEvents || !Properties.VibrateOnCameraShake)
                return;

            ButtplugManager.VibrateRelativeMin(__instance.CurrentStrength, __instance.ShakeParameters.strength, Properties.MinSpeed, Properties.ScreenshakeMultiplier);
        }
    }

    /// <summary>
    /// Patches skill activation to trigger vibration
    /// </summary>
    [HarmonyPatch(typeof(PlayerCasting), nameof(PlayerCasting.Client_InitCastParams))]
	public static class CastSkillPatch
	{
		[HarmonyPrefix]
		public static void InitCast(PlayerCasting __instance, string _skillName) {
            if (!Properties.ForwardPatchedEvents || !Properties.VibrateOnCameraShake)
                return;

            if (__instance._player == null || !__instance._player.isLocalPlayer)
                return;

            ButtplugManager.Tap();
        }
    }

    /// <summary>
    /// Patches health regen to trigger vibration relative to healed amount 
    /// </summary>
    [HarmonyPatch(typeof(StatusEntity), nameof(StatusEntity.Add_Health))]
	public static class AddHealthPatch
	{
		[HarmonyPrefix]
		public static void AddHealth_Prefix(StatusEntity __instance, int _value, out float __state) {
            ButtplugManager.Tap();
            __state = 0;

            if (!Properties.ForwardPatchedEvents || !Properties.VibrateOnHeal)
                return;

            if (__instance._isPlayer == null || !__instance._isPlayer.isLocalPlayer)
                return;

            __state = __instance._currentHealth;
        }

		[HarmonyPostfix]
		public static void AddHealth_Postfix(StatusEntity __instance, int _value, float __state) {
            ButtplugManager.Tap();
            if (!Properties.ForwardPatchedEvents || !Properties.VibrateOnHeal)
                return;

            if (__instance._isPlayer == null || !__instance._isPlayer.isLocalPlayer)
                return;

            float healAmount = __instance._currentHealth - __state;
            // Vibrate relative to health added to original amount, don't vibrate if none
            ButtplugManager.VibrateRelativeMin(Mathf.Abs(healAmount), __state, 0);
        }
    }

    /// <summary>
    /// Patches health subtraction to vibrate relative to current health 
    /// </summary>
    [HarmonyPatch(typeof(StatusEntity), nameof(StatusEntity.Subtract_health))]
    public static class SubtractHealthPatch {
		[HarmonyPrefix]
		public static void SubtractHealth(StatusEntity __instance, int _value) {
            if (!Properties.ForwardPatchedEvents || !Properties.VibrateOnHeal)
                return;

            if (__instance._isPlayer == null || !__instance._isPlayer.isLocalPlayer)
                return;

            if(_value >= 0)
                ButtplugManager.VibrateRelativeMin(Mathf.Abs(_value), __instance._currentHealth, Properties.MinSpeed);
        }
    }

    /// <summary>
    /// Patches change in stamina to vibrate relative to current stamina 
    /// </summary>hange_Stamina))]
    [HarmonyPatch(typeof(StatusEntity), nameof(StatusEntity.Change_Stamina))]
	public static class ChangeStaminaPatch
	{
		[HarmonyPrefix]
		public static void ChangeStamina(StatusEntity __instance, int _value) {
            if (!Properties.ForwardPatchedEvents || !Properties.VibrateOnStaminaUse)
                return;

            if (__instance._isPlayer == null || !__instance._isPlayer.isLocalPlayer)
                return;

            if (_value > 0) {
                float changeAmount = Mathf.Abs(_value - __instance._currentStamina);
                ButtplugManager.VibrateRelative(changeAmount, __instance._currentStamina);
            }
        }
    }

    [HarmonyPatch(typeof(StatusEntity), nameof(StatusEntity.Change_Mana))]
	public static class ChangeManaPatch
	{
		[HarmonyPrefix]
		public static void Change_Mana(StatusEntity __instance, int _value) {
            if (!Properties.ForwardPatchedEvents)
                return;

            if (__instance._isPlayer == null || !__instance._isPlayer.isLocalPlayer)
                return;

            if (_value > 0) {
                float changeAmount = Mathf.Abs(_value - __instance._currentMana);
                ButtplugManager.VibrateRelative(changeAmount, __instance._currentMana);
            }
        }
    }

    /// <summary>
    /// Patches damage applied by the player to vibrate relative to health of target
    /// </summary>
    [HarmonyPatch(typeof(CombatCollider), nameof(CombatCollider.Apply_Damage))]
	public static class DealDamagePatch
	{
		[HarmonyPrefix]
		public static void Apply_Damage(CombatCollider __instance, ITakeDamage _damageable, int _passedLevel, int _damageValue, bool _isCriticalHit, ScriptableCombatElement _combatElement, Vector3 _hitPoint) {
            if (!Properties.ForwardPatchedEvents || !Properties.VibrateOnDealDamage)
                return;

            // Only care about damage dealt by THIS player
            if (!__instance._isParentPlayer || !__instance._isParentPlayer.isLocalPlayer)
                return;

            if (_isCriticalHit)
                ButtplugManager.Vibrate(1);
            else {
                // Vibrate relative to hit entity's current health
                if(__instance._hitCreep != null)
                    ButtplugManager.VibrateRelativeMin(Mathf.Abs(_damageValue), __instance._hitCreep._statusEntity._currentHealth, Properties.MinSpeed);
                else if (__instance._hitPlayer != null)
                    ButtplugManager.VibrateRelativeMin(Mathf.Abs(_damageValue), __instance._hitPlayer._playerStatus._currentHealth, Properties.MinSpeed);
            }
        }
    }
}