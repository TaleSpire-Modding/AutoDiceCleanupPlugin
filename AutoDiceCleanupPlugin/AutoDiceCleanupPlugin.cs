using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using ModdingTales;
using UnityEngine;
using System.Collections;
using Bounce;
using TMPro;
using UnityEngine.Serialization;

namespace AutoDiceCleanup
{
    [BepInPlugin(Guid, "Auto Dice Cleanup Plugin", Version)]
    public class AutoDiceCleanupPlugin : BaseUnityPlugin
    {
        // constants
        public const string Guid = "org.hollofox.plugins.AutoDiceCleanup";
        internal const string Version = "1.0.0.0";

        public static AutoDiceCleanupPlugin Instance;
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
            yield return new WaitForSeconds(Mathf.Max(1,delayInSeconds)); // Minimum of 1s delay
            PhotonSimpleSingletonBehaviour<DiceManager>.Instance.ClearDiceRoll(rollId);
        }

        /// <summary>
        /// Awake plugin
        /// </summary>
        void Awake()
        {
            Instance = this;

            Debug.Log("Auto Dice Cleanup loaded");
            _cleanupAfterSelf = Config.Bind("Cleanup", "Self", true);
            _cleanupAsGm = Config.Bind("Cleanup", "All as GM", true);

            _cleanupAfterSelfInSeconds = Config.Bind("Cleanup", "Self Delay in Seconds", 5f);
            _cleanupAsGmInSeconds = Config.Bind("Cleanup", "GM Delay in Seconds", 5f);

            ModdingUtils.Initialize(this, Logger, "HolloFoxes'");
            var harmony = new Harmony(Guid);
            harmony.PatchAll();
        }
    }
}
