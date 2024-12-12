using HarmonyLib;

namespace BUTTLYSS
{
    /// <summary>
    /// Patches item addition to inventory to trigger tap
    /// </summary>
    [HarmonyPatch(typeof(PlayerInventory), "Add_Item")]
    public static class AddItempatch
    {
    	static void Postfix(ItemData _itemData) {
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
    	static void Postfix(ItemData _itemData, int _quantity) {
            if (!Properties.ForwardPatchedEvents)
                return;
            ButtplugManager.Tap();
        }
    }
}