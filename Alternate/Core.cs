using MelonLoader;
using HarmonyLib;
using Newtonsoft.Json;
using ScheduleOne.Levelling;
using ScheduleOne.Economy;
using ScheduleOne.UI;
using MelonLoader.Utils;

[assembly: MelonInfo(typeof(ScheduleOne_XPAdjuster.Core), "ScheduleOne_XPAdjuster", "1.0.2", "Apatel78", null)]
[assembly: MelonGame("TVGS", "Schedule I")]

namespace ScheduleOne_XPAdjuster
{
    public static class ModConfig
    {
        public static string config_path = System.IO.Path.Combine(MelonEnvironment.UserDataDirectory, "XPAdjuster");
        private static MelonPreferences_Category XPCategory;
        public static MelonPreferences_Entry<int> PlayerCompletedDeal;
        public static MelonPreferences_Entry<int> DealerCompletedDeal;
        public static MelonPreferences_Entry<int> EscapedWanted;
        public static MelonPreferences_Entry<int> EscapedWanted2;
        public static MelonPreferences_Entry<int> EscapedArrest;

        public static void CreateDirectory()
        {
            Directory.CreateDirectory(config_path);
            Init();
        }

        public static void Init()
        {
            XPCategory = MelonPreferences.CreateCategory("XP Adjustments");
            PlayerCompletedDeal = XPCategory.CreateEntry<int>("Player Completed Deal XP", 20, "XP awarded when the player completes a deal.");
            DealerCompletedDeal = XPCategory.CreateEntry<int>("Dealer Completed Deal XP", 10, "XP awarded when a dealer completes a deal.");
            EscapedWanted = XPCategory.CreateEntry<int>("Escaped Wanted Level 1 XP", 40, "XP awarded for escaping wanted.");
            EscapedWanted2 = XPCategory.CreateEntry<int>("Escaped Wanted Level 2 XP", 60, "XP awarded for escaping wanted deal or alive.");
            EscapedArrest = XPCategory.CreateEntry<int>("Escaped Arrest XP", 20, "XP awarded for escaping under arrest.");

            XPCategory.SetFilePath(System.IO.Path.Combine(config_path, "XPAdjusterConfig.cfg"));
            XPCategory.SaveToFile();
        }
    }

    public class Core : MelonMod
    {
        public static int PlayerCompletedDealXP;
        public static int DealerCompletedDealXP;
        public static int EscapedWantedXP;
        public static int EscapedWanted2XP;
        public static int EscapedArrestXP;
        public override void OnInitializeMelon()
        {
            var harmony = new HarmonyLib.Harmony("com.apatel.schedule1.xppatch");

            harmony.PatchAll();

            ModConfig.CreateDirectory();

            PlayerCompletedDealXP = ModConfig.PlayerCompletedDeal.Value;
            DealerCompletedDealXP = ModConfig.DealerCompletedDeal.Value;
            EscapedWantedXP = ModConfig.EscapedWanted.Value;
            EscapedWanted2XP = ModConfig.EscapedWanted2.Value;
            EscapedArrestXP = ModConfig.EscapedArrest.Value;
        }

        [HarmonyPatch(typeof(LevelManager), "AddXP")]
        public class Patch_LevelManager_AddXP
        {
            static bool Prefix(ref int xp)
            {
                if (xp == 10)
                {
                    xp = Core.DealerCompletedDealXP;
                    // MelonLogger.Msg($"XP overridden for DEALER_COMPLETED_DEAL: 10 -> {xp}");
                    PlayerDealChecker.IsProcessHandoverCalled = false;
                    // XPEarnedTracker.DealerDeals++;
                }
                else if (xp == 20)
                {
                    if (PlayerDealChecker.IsProcessHandoverCalled)
                    {
                        xp = Core.PlayerCompletedDealXP;
                        // MelonLogger.Msg($"XP overridden for PLAYER_COMPLETED_DEAL: 20 -> {xp}");
                        PlayerDealChecker.IsProcessHandoverCalled = false;
                        // XPEarnedTracker.PlayerDeals++;
                    }
                    else
                    {
                        xp = Core.EscapedArrestXP;
                        // MelonLogger.Msg($"XP overridden for ESCAPED_ARREST: 20 -> {xp}");
                        // XPEarnedTracker.EscapedArrests++;
                    }
                }
                else if (xp == 40)
                {
                    xp = Core.EscapedWantedXP;
                    // MelonLogger.Msg($"XP overridden for ESCAPED_WANTED: 40 -> {xp}");
                    // XPEarnedTracker.EscapedWanteds++;
                }
                else if (xp == 60)
                {
                    xp = Core.EscapedWanted2XP;
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