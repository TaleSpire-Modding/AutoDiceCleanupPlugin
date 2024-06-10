using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;

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

    [HarmonyPatch(typeof(DiceManager), "RPC_DiceResult")]
    public class InitiativeDiceRolledPatch
    {
        private static Dictionary<CreatureGuid, decimal> initiatives = new Dictionary<CreatureGuid, decimal>();

        internal static void orderedInitiatives() {
            InitiativeManager.Instance.SyncQueueList(initiatives.OrderBy(i => i.Value).Select(i => i.Key).ToArray());
        }

        internal static void trackInitiative(CreatureGuid creature, int totalValue, int modifier)
        {
            decimal initiativeValue = ((decimal)0.01 * modifier) + totalValue;
            if (initiatives.ContainsKey(creature))
            {
                initiatives[creature] = initiativeValue;
            }
            else
            {
                initiatives.Add(creature, initiativeValue);
            }
            orderedInitiatives();
        }

        internal static void removeInitiative(CreatureGuid creature)
        {
            initiatives.Remove(creature);
            orderedInitiatives();
        }

        static bool SearchOperand(DiceManager.RollOperand operand, out DiceManager.RollResult result)
        {
            DiceManager.RollResultsOperation operation2;
            DiceManager.RollValue value;
            DiceManager.RollOperand.Which which = operand.Get(out operation2, out result, out value);
            if ((ulong)which <= 2uL)
            {
                switch (which)
                {
                    case DiceManager.RollOperand.Which.Operation:
                        return SearchOperation(operation2, out result);
                    case DiceManager.RollOperand.Which.Result:
                        return result.Results.Length == 1;
                    case DiceManager.RollOperand.Which.Value:
                        return false;
                }
            }

            throw new ArgumentOutOfRangeException();
        }

        static bool SearchOperation(DiceManager.RollResultsOperation operation, out DiceManager.RollResult result)
        {
            if (operation.Operator == DiceManager.DiceOperator.Add && operation.Operands.Length == 1)
            {
                return SearchOperand(operation.Operands[0], out result);
            }

            result = default;
            return false;
        }

        public static bool TryFindSingleRollResult(DiceManager.RollResultsGroup resultsGroup, out DiceManager.RollResult result)
        {
            return SearchOperand(resultsGroup.Result, out result);
        }

        static void Postfix(bool isGmOnly, byte[] diceListData, PhotonMessageInfo msgInfo, BrSerialize.Reader ____reader)
        {
            AutoDiceCleanupPlugin.logger.LogDebug($"Dice Rolled");

            if (LocalClient.IsInGmMode && BoardToolManager.Instance.IsCurrentTool<TurnQueueEditBoardTool>())
            {
                AutoDiceCleanupPlugin.logger.LogDebug($"In TurnQueueEditBoardTool");

                BrSerializeHelpers.DeserializeFromByteArray(____reader, diceListData, DiceManager.RollResults.Deserialize, out DiceManager.RollResults rollResultData);

                AutoDiceCleanupPlugin.logger.LogDebug($"Serialized");

                DiceManager.RollResultsGroup[] bufferUnsafe = rollResultData.ResultsGroups.GetBufferUnsafe(out int count);

                AutoDiceCleanupPlugin.logger.LogDebug($"Buffer Created");

                for (int i = 0; i < count; i++)
                {
                    AutoDiceCleanupPlugin.logger.LogDebug($"{bufferUnsafe[i].Name}: {bufferUnsafe[i].Result}");
                }
            }
        }
    }


}
