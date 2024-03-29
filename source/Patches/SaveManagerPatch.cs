﻿using HarmonyLib;
using Reactor;

namespace BetterTownOfUs.RainbowMod
{
    [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.GetPrefsName))]
    public class SaveManagerPatch
    {
        public static void Postfix(ref string __result)
        {
            __result += "_BTOU";
        }
    }
}
