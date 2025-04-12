using MelonLoader;
using HarmonyLib;
using Newtonsoft.Json;
using ScheduleOne.Levelling;
using ScheduleOne.Economy;
using ScheduleOne.UI;

[assembly: MelonInfo(typeof(ScheduleOne_XPAdjuster.Core), "ScheduleOne_XPAdjuster", "1.0.0", "Apatel78", null)]
[assembly: MelonGame("TVGS", "Schedule I")]

namespace ScheduleOne_XPAdjuster
{
    public class Core : MelonMod
    {
        public override void OnInitializeMelon()
        {
            var harmony = new HarmonyLib.Harmony("com.apatel.schedule1.xppatch");
            harmony.PatchAll();

            MelonLogger.Msg("Schedule1 XP Patch initialized!");


            var config = ConfigCache.XP;

            if (config != null)
            {
                MelonLogger.Msg($"Loaded XP config: PLAYER_COMPLETED_DEAL = {config.PLAYER_COMPLETED_DEAL}, DEALER_COMPLETED_DEAL = {config.DEALER_COMPLETED_DEAL}, ESCAPED_ARREST = {config.ESCAPED_ARREST}, ESCAPED_WANTED = {config.ESCAPED_WANTED}, ESCAPED_WANTED2 = {config.ESCAPED_WANTED2}");
            }
            else
            {
                MelonLogger.Msg("No config file found, using default XP values.");
            }

        }
        public class XPConfig
        {
            public int PLAYER_COMPLETED_DEAL { get; set; }
            public int DEALER_COMPLETED_DEAL { get; set; }
            public int ESCAPED_WANTED { get; set; }
            public int ESCAPED_WANTED2 { get; set; }
            public int SUCCESSFUL_SAMPLE { get; set; }
            public int HARVESTED_PLANT { get; set; }
            public int ESCAPED_ARREST { get; set; }

            // Load the XPConfig from a JSON file
            public static XPConfig Load(string filePath)
            {
                if (!File.Exists(filePath))
                {
                    MelonLogger.Msg($"Config file not found at {filePath}, using defaults.");
                    return null; // Return null if not found
                }

                string json = File.ReadAllText(filePath);
                MelonLogger.Msg($"Loaded JSON: {json}"); // Log the raw JSON to debug
                return JsonConvert.DeserializeObject<XPConfig>(json);
            }
        }

        public static class ConfigCache
        {
            public static XPConfig XP { get; private set; }

            static ConfigCache()
            {
                XP = XPConfig.Load("Mods/config/xp_config.json"); // Load from the specified path
            }
        }

        [HarmonyPatch(typeof(LevelManager), "AddXP")]
        public class Patch_LevelManager_AddXP
        {
            static bool Prefix(ref int xp)
            {
                var config = ConfigCache.XP;

                if (xp == 10)
                {
                    xp = config?.DEALER_COMPLETED_DEAL ?? 10;
                    // MelonLogger.Msg($"XP overridden for DEALER_COMPLETED_DEAL: 10 -> {xp}");
                    PlayerDealChecker.IsProcessHandoverCalled = false;
                    // XPEarnedTracker.DealerDeals++;
                }
                else if (xp == 20)
                {
                    if (PlayerDealChecker.IsProcessHandoverCalled)
                    {
                        xp = config?.PLAYER_COMPLETED_DEAL ?? 20;
                        // MelonLogger.Msg($"XP overridden for PLAYER_COMPLETED_DEAL: 20 -> {xp}");
                        PlayerDealChecker.IsProcessHandoverCalled = false;
                        // XPEarnedTracker.PlayerDeals++;
                    }
                    else
                    {
                        xp = config?.ESCAPED_ARREST ?? 20;
                        // MelonLogger.Msg($"XP overridden for ESCAPED_ARREST: 20 -> {xp}");
                        // XPEarnedTracker.EscapedArrests++;
                    }
                }
                else if (xp == 40)
                {
                    xp = config?.ESCAPED_WANTED ?? 40;
                    // MelonLogger.Msg($"XP overridden for ESCAPED_WANTED: 40 -> {xp}");
                    // XPEarnedTracker.EscapedWanteds++;
                }
                else if (xp == 60)
                {
                    xp = config?.ESCAPED_WANTED2 ?? 60;
                    // MelonLogger.Msg($"XP overridden for ESCAPED_WANTED2: 60 -> {xp}");
                    // XPEarnedTracker.EscapedWanteds2++;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(Customer), "ProcessHandover")]
        public class Patch_Customer_ProcessHandover
        {
            static void Prefix(Customer __instance)
            {
                PlayerDealChecker.IsProcessHandoverCalled = true;
                // MelonLogger.Msg($"ProcessHandover called on customer: {__instance.name}");
            }
        }
        public static class PlayerDealChecker
        {
            public static bool IsProcessHandoverCalled = false;
        }

        //[HarmonyPatch(typeof(SleepCanvas), "SleepStart")]

        //public class Patch_SleepCanvas_SleepStart
        //{
        //    static void Postfix()
        //    {
        //        MelonLogger.Msg($"XP from Player Deals: {XPEarnedTracker.PlayerDeals}");
        //        MelonLogger.Msg($"XP from Customer Deals: {XPEarnedTracker.DealerDeals}");
        //        MelonLogger.Msg($"XP from Evading Arrest: {XPEarnedTracker.EscapedArrests}");
        //        MelonLogger.Msg($"XP from Evading Wanted: {XPEarnedTracker.EscapedWanteds}");
        //        MelonLogger.Msg($"XP from Evading Wanted2: {XPEarnedTracker.EscapedWanteds2}");

        //        XPEarnedTracker.Reset();
        //    }
        //}

        //public static class XPEarnedTracker
        //{
        //    public static int PlayerDeals = 0;
        //    public static int DealerDeals = 0;
        //    public static int EscapedArrests = 0;
        //    public static int EscapedWanteds = 0;
        //    public static int EscapedWanteds2 = 0;

        //    public static void Reset()
        //    {
        //        PlayerDeals = 0;
        //        DealerDeals = 0;
        //        EscapedArrests = 0;
        //        EscapedWanteds = 0;
        //        EscapedWanteds2 = 0;
        //    }
        //}

    }
}