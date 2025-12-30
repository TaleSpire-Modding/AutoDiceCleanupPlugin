using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using System.Collections;
using BepInEx.Logging;
using PluginUtilities;

namespace AutoDiceCleanup
{
    [BepInPlugin(Guid, "Auto Dice Cleanup Plugin", Version)]
    [BepInDependency(SetInjectionFlag.Guid)]
    public class AutoDiceCleanupPlugin : DependencyUnityPlugin
    {
        // constants
        public const string Guid = "org.hollofox.plugins.AutoDiceCleanup";
        internal const string Version = "0.0.0.0";

        public static AutoDiceCleanupPlugin Instance;
        internal static ManualLogSource logger;
        private static ConfigEntry<bool> _cleanupAfterSelf { get; set; }

        internal static bool CleanUpAfterSelf
        {
            get => _cleanupAfterSelf.Value;
            set => _cleanupAfterSelf.Value = value;
        }

        private static ConfigEntry<bool> _cleanupAsGm { get; set; }

        internal static bool CleanupAsGm
        {
            get => _cleanupAsGm.Value;
            set => _cleanupAsGm.Value = value;
        }

        private static ConfigEntry<float> _cleanupAfterSelfInSeconds { get; set; }

        internal static float CleanupAfterSelfInSeconds
        {
            get => _cleanupAfterSelfInSeconds.Value;
            set => _cleanupAfterSelfInSeconds.Value = value;
        }

        private static ConfigEntry<float> _cleanupAsGmInSeconds { get; set; }

        internal static float CleanupAsGmInSeconds
        {
            get => _cleanupAsGmInSeconds.Value;
            set => _cleanupAsGmInSeconds.Value = value;
        }

        internal void ClearDiceRoll(float delayInSeconds, RollId rollId)
        {
            StartCoroutine(ClearDiceRollIEnum(delayInSeconds, rollId));
        }

        internal IEnumerator ClearDiceRollIEnum(float delayInSeconds, RollId rollId)
        {
            yield return new WaitForSeconds(Mathf.Max(1, delayInSeconds)); // Minimum of 1s delay
            try
            {
                logger.LogDebug($"Clearing dice roll {rollId}");
                Dice.DiceRollManager.Instance.RemoveRoll(rollId);
            }
            catch (System.Exception e) { 
                // Exception may be thrown if the dice roll is already cleared
                logger.LogDebug(e.Message); 
            }
        }

        Harmony harmony;

        /// <summary>
        /// Awake plugin
        /// </summary>
        protected override void OnAwake()
        {
            Instance = this;
            logger = Logger;

            Logger.LogDebug("Auto Dice Cleanup loaded");
            _cleanupAfterSelf = Config.Bind("Cleanup", "Self", true);
            _cleanupAsGm = Config.Bind("Cleanup", "All as GM", true);

            _cleanupAfterSelfInSeconds = Config.Bind("Cleanup", "Self Delay in Seconds", 5f);
            _cleanupAsGmInSeconds = Config.Bind("Cleanup", "GM Delay in Seconds", 5f);

            try {
                harmony = new Harmony(Guid);
                harmony.PatchAll();
            }
            catch (System.Exception e)
            {
                logger.LogError(e.Message);
            }
        }

        protected override void OnDestroyed()
        {
            harmony.UnpatchSelf();
            Instance = null;
            logger = null;
            harmony = null;

            Logger.LogDebug("Auto Dice Cleanup unloaded");
        }
    }
}
