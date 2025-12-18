using Bounce.Unmanaged;
using Dice;
using GameChat.UI;
using HarmonyLib;

namespace AutoDiceCleanup.Patches
{

    [HarmonyPatch(typeof(DiceRollManager), "OnResults")]
    public class OnDiceRolledPatch
    {
        static void Postfix(DiceRollManager __instance, 
            ClientGuid clientId, 
            RollResults rollResults, 
            bool isGmRoll, 
            bool showResult, 
            UIChatMessageManager.DiceResultsReference.ResultsOrigin resultsOrigin, 
            NGuid optionalSymbioteInteropId)
        {
            if (LocalClient.IsInGmMode && AutoDiceCleanupPlugin.CleanupAsGm)
            {
                AutoDiceCleanupPlugin.Instance.ClearDiceRoll(AutoDiceCleanupPlugin.CleanupAsGmInSeconds, rollResults.RollId);
            }
            else if(clientId == LocalClient.Id && AutoDiceCleanupPlugin.CleanUpAfterSelf)
            {
                AutoDiceCleanupPlugin.Instance.ClearDiceRoll(AutoDiceCleanupPlugin.CleanupAfterSelfInSeconds, rollResults.RollId);
            }
        }
    }

}
