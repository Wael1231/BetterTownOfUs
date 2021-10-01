using System;
using HarmonyLib;
using BetterTownOfUs.Roles;

namespace BetterTownOfUs.ImpostorRoles.CamouflageMod
{
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Start))]
    public static class Start
    {
        public static void Postfix(ShipStatus __instance)
        {
            foreach (var role in Role.GetRoles(RoleEnum.Camouflager))
            {
                var seer = (Camouflager) role;
                seer.LastCamouflaged = DateTime.UtcNow;
            }
        }
    }
}