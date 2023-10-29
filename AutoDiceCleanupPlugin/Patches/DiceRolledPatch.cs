using HarmonyLib;
using UnityEngine;

namespace AutoDiceCleanup.Patches
{
    
    [HarmonyPatch(typeof(DiceManager), "SendDiceResult")]
    public class SendDiceRolledPatch
    {
        static void Postfix(bool isGmOnly, DiceManager.RollResults rollResultData)
        {
            if (AutoDiceCleanupPlugin.CleanUpAfterSelf) 
            {
                AutoDiceCleanupPlugin.Instance.ClearDiceRoll(AutoDiceCleanupPlugin.CleanupAfterSelfInSeconds, rollResultData.RollId);
            } 
        }
    }

    [HarmonyPatch(typeof(DiceManager), "RPC_DiceResult")]
    public class ReceiveDiceRolledPatch
    {
        static void Postfix(bool isGmOnly, byte[] diceListData, PhotonMessageInfo msgInfo, BrSerialize.Reader ____reader)
        {
            if (LocalClient.IsInGmMode && AutoDiceCleanupPlugin.CleanupAsGm)
            {
                BrSerializeHelpers.DeserializeFromByteArray(____reader, diceListData, DiceManager.RollResults.Deserialize, out DiceManager.RollResults thing);
                AutoDiceCleanupPlugin.Instance.ClearDiceRoll(AutoDiceCleanupPlugin.CleanupAsGmInSeconds, thing.RollId);
            }
        }
    }
}
