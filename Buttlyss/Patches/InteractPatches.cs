using HarmonyLib;

namespace BUTTLYSS.Patches
{
    /// <summary>
    /// Patches item addition to inventory to trigger tap
    /// </summary>
    [HarmonyPatch(typeof(PlayerInventory), "Add_Item")]
    public static class AddItempatch
    {
        [HarmonyPostfix]
    	static void AddItem(PlayerInventory __instance, ItemData _itemData) {
            if (!Properties.ForwardPatchedEvents || !Properties.VibrateOnItemAddRemove)
                return;

            if (__instance.isLocalPlayer)
                ButtplugManager.Tap();
        }
    }

    /// <summary>
    /// Patches item removal from inventory to trigger tap
    /// </summary>
    [HarmonyPatch(typeof(PlayerInventory), "Remove_Item")]
    public static class RemoveItemPatch
    {
        [HarmonyPostfix]
    	static void RemoveItem(PlayerInventory __instance, ItemData _itemData, int _quantity) {
            if (!Properties.ForwardPatchedEvents || !Properties.VibrateOnItemAddRemove)
                return;

            if (__instance.isLocalPlayer)
                ButtplugManager.Tap();
        }
    }
}