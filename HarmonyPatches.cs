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
    //         if (!ButtplugManager.ForwardPatchedEvents) return;
    //         ButtplugManager.Vibrate(_result.);
    //     }
    // }

    // patch private OnPointerClick
    [HarmonyPatch(typeof(Button), "Press")]
    public class ButtonPatch
    {
        static void Postfix()
        {
            if (!ButtplugManager.ForwardPatchedEvents) return;
            ButtplugManager.Tap(true);
        }
    }

    [HarmonyPatch(typeof(PlayerMove), nameof(PlayerMove.Set_MovementAction))]
    public class MovementPatch
    {
        static MovementAction Postfix(MovementAction _mA)
        {
            if (!ButtplugManager.ForwardPatchedEvents) return _mA;
            switch (_mA)
            {
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

            return _mA;
        }
    }
}