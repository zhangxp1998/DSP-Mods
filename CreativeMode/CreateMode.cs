using BepInEx;
using HarmonyLib;
using System;
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
        }
        internal void OnDestroy()
        {
            harmony.UnpatchSelf();  // For ScriptEngine hot-reloading
        }
        internal void Update()
        {
            if (Input.GetKeyDown(KeyCode.R)) 
            {
                creativeModeEnabled = !creativeModeEnabled;
                if (creativeModeEnabled)
                {
                    UIRealtimeTip.PopupAhead("Creative mode enabled!");
                } else
                {
                    UIRealtimeTip.PopupAhead("Creative mode disabled!");
                }

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
