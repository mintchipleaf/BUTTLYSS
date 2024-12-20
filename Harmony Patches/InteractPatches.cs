using HarmonyLib;

namespace BUTTLYSS
{
    /// <summary>
    /// Patches item addition to inventory to trigger tap
    /// </summary>
    [HarmonyPatch(typeof(PlayerInventory), "Add_Item")]
    public static class AddItempatch
    {
        [HarmonyPostfix]
    	static void AddItem(ItemData _itemData) {
            if (!Properties.ForwardPatchedEvents)
                return;
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
    	static void RemoveItem(ItemData _itemData, int _quantity) {
            if (!Properties.ForwardPatchedEvents)
                return;
            ButtplugManager.Tap();
        }
    }
}