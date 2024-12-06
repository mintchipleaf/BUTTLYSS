using System.Collections.Generic;
using HarmonyLib;
using MilkShake;

using UnityEngine.UI;

namespace BUTTLYSS
{
    // [HarmonyPatch(typeof(ShakeInstance), nameof(ShakeInstance.UpdateShake))]
    // public class CameraPatch
    // {
    //     static void Postfix(ShakeResult _result)
    //     {
    //         if (!ButtlyssProperties.ForwardPatchedEvents) return;
    //         ButtplugManager.Vibrate(1);
    //     }
    // }

    // patch private OnPointerClick
    [HarmonyPatch(typeof(Button), "Press")]
    public class ButtonPatch
    {
        static void Postfix() {
            if (!ButtlyssProperties.ForwardPatchedEvents)
                return;
            ButtplugManager.Tap(true);
        }
    }

    [HarmonyPatch(typeof(PlayerMove), nameof(PlayerMove.Set_MovementAction))]
    public class MovementPatch
    {
        static void Postfix(MovementAction _mA) {
            if (!ButtlyssProperties.ForwardPatchedEvents)
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
}