using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CreativeMode
{
    public static class Constants
    {
        public const string GUID = "402cb1c4-a303-4ac0-9f8c-e1104cefc59a";
    }

    [BepInPlugin(Constants.GUID, "CreateMode", "0.5.0")]
    public class CreateMode : BaseUnityPlugin
    {
        Harmony harmony;
        static bool creativeModeEnabled = true;
        internal void Awake()
        {
            harmony = new Harmony(Constants.GUID);
            harmony.PatchAll(typeof(PatchStorageComponent));
            // harmony.PatchAll(typeof(PatchFlattenTerrainReform));
            System.Console.WriteLine("Build area = " + GameMain.mainPlayer.mecha.buildArea);
            GameMain.mainPlayer.mecha.buildArea = 30000;
            GameMain.mainPlayer.mecha.droneSpeed = 30000;
        }
        internal void OnDestroy()
        {
            harmony.UnpatchSelf();  // For ScriptEngine hot-reloading
        }
        internal void Update()
        {
            if (Input.GetKeyDown(KeyCode.I))
            {
                creativeModeEnabled = !creativeModeEnabled;
                if (creativeModeEnabled)
                {
                    UIRealtimeTip.PopupAhead("Creative mode enabled!");
                }
                else
                {
                    UIRealtimeTip.PopupAhead("Creative mode disabled!");
                }
            }
            if (Input.GetKeyDown(KeyCode.O))
            {
                Logger.LogInfo("I see that Hotkey is pressed!!!");

                if (UIRoot.instance == null)
                {
                    Logger.LogInfo("Can't find UIRoot... !");
                    return;
                }
                GameHistoryData history = UIRoot.instance.uiGame.spaceGuide.history;
                if (history == null)
                {
                    Logger.LogInfo("No Game history data found?!");
                    return;
                }
                if (history.techQueueLength > 0)
                {
                    int[] techQueue = new int[history.techQueue.Length];
                    history.techQueue.CopyTo(techQueue, 0);
                    for (int i = 0; i < techQueue.Length; i++)
                    {
                        int tech = techQueue[i];
                        history.AddTechHash(history.TechState(tech).hashNeeded);
                        TechState state = history.TechState(tech);
                        Logger.LogInfo("Tech id: " + tech + " Tech Level: " + state.curLevel);
                    }
                }
                Logger.LogInfo("Unlocking all upgrades!");
                int[,] techs = new int[,]{ 
                // Universe exploration
                    {4101, 4104 },
                // Mecha Core
                    {2101, 2105 },
                // Mechnical Frame
                    {2201, 2208 },
                // Inventory Capacity
                    {2301, 2306 },
                // Communication Control
                    {2401, 2406 },
                // Energy Circuit
                    {2501, 2505 },
                // Drone Engine
                    {2601, 2605 },
                // Drive Engine
                    {2901, 2903 },
                // Solar Sail Life
                    {3101, 3105 },
                // Ray Transmission Efficiency
                    {3201, 3207},
                // Vertical Construction
                    {3701, 3706 },
                // 	Sorter Cargo Stacking
                    {3301, 3305 },
                // Logistics Carrier Engine
                    {3401, 3406 },
                // Logistics Carrier Capacity
                    {3501, 3508 },
                // Veins Utilization
                    {3601, 3605 },
                // Research Speed
                    {3901, 3903 }
                };
                for (int i = 0; i < techs.GetLength(0); i++)
                {
                    for (int j = techs[i, 0]; j <= techs[i, 1]; j++)
                        if (!history.TechUnlocked(j))
                        {
                            history.UnlockTech(j);
                        }
                }
            }
        }

        public static class PatchFlattenTerrainReform
        {
            public static int reformType = 0;
            public static int reformColor = 0;

            [HarmonyPrefix]
            [HarmonyPatch(typeof(PlayerAction_Build), "DetermineBuildPreviews")]
            public static void DetermineBuildPreviews__Postfix(PlayerAction_Build __instance)
            {
                reformType = __instance.reformType;
                reformColor = __instance.reformColor;
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(PlanetFactory), "FlattenTerrainReform")]
            public static bool PlanetFactory__FlattenTerrainReform__Prefix(PlanetFactory __instance, 
                Vector3 center,
                ref float radius,
                ref int reformSize,
                bool veinBuried,
                float fade0)
            {
                if (Input.GetKey(KeyCode.LeftControl))
                {
                    System.Console.WriteLine("Huge reform!");
                    reformSize *= 100;
                    radius *= 100;
                    PlatformSystem platformSystem = __instance.platformSystem;
                    for (int i = 0; i < platformSystem.maxReformCount; i++)
                    {
                        platformSystem.SetReformType(i, reformType);
                        platformSystem.SetReformColor(i, reformColor);
                    }
                } else
                {
                    System.Console.WriteLine("Regular reform");
                }
                return true;
            }
        }

        public static class PatchStorageComponent
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(StorageComponent), "TakeTailItems")]
            [HarmonyPatch(new Type[] { typeof(int), typeof(int), typeof(bool) }, new ArgumentType[] { ArgumentType.Ref, ArgumentType.Ref, ArgumentType.Normal })]
            public static bool StorageComponent__TakeTailItems__Prefix(StorageComponent __instance, ref int itemId, ref int count)
            {
                return TakeTailItems__Prefix(__instance, ref itemId, ref count);
            }
            [HarmonyPrefix]
            [HarmonyPatch(typeof(StorageComponent), "TakeTailItems")]
            [HarmonyPatch(new Type[] { typeof(int), typeof(int), typeof(int[]), typeof(bool) }, new ArgumentType[] { ArgumentType.Ref, ArgumentType.Ref, ArgumentType.Normal, ArgumentType.Normal })]
            public static bool StorageComponent__TakeTailItemsArr__Prefix(StorageComponent __instance, ref int itemId, ref int count)
            {
                return TakeTailItems__Prefix(__instance, ref itemId, ref count);
            }
            [HarmonyPrefix]
            [HarmonyPatch(typeof(StorageComponent), "TakeHeadItems")]
            [HarmonyPatch(new Type[] { typeof(int), typeof(int) }, new ArgumentType[] { ArgumentType.Ref, ArgumentType.Ref })]
            public static bool StorageComponent__TakeHeadItems__Prefix(StorageComponent __instance, ref int itemId, ref int count)
            {
                return TakeTailItems__Prefix(__instance, ref itemId, ref count);
            }
            [HarmonyPrefix]
            [HarmonyPatch(typeof(StorageComponent), "TakeItem")]
            public static bool StorageComponent__TakeItem__Prefix(StorageComponent __instance, int itemId, int count)
            {
                return TakeTailItems__Prefix(__instance, ref itemId, ref count);
            }

            public static bool TakeTailItems__Prefix(StorageComponent __instance, ref int itemId, ref int count)
            {
                if (!creativeModeEnabled)
                {
                    return true;
                }
                if (itemId != 0 && count != 0)
                {
                    return false;
                }
                var grids = __instance.grids;
                for (int i = __instance.grids.Length - 1; i >= 0; i--)
                {
                    if (grids[i].itemId == 0)
                    {
                        continue;
                    }
                    if (itemId == 0 || itemId == grids[i].itemId)
                    {
                        itemId = grids[i].itemId;
                        count = Math.Max(count, 1);
                        return false;
                    }
                }
                return true;
            }
        }

    }
}
