using System;
using HarmonyLib;
using BetterTownOfUs.Roles;

namespace BetterTownOfUs.NeutralRoles.ShifterMod
{
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Start))]
    public static class Start
    {
        public static void Postfix(ShipStatus __instance)
        {
            foreach (var role in Role.GetRoles(RoleEnum.Shifter))
            {
                var shifter = (Shifter) role;
                shifter.LastShifted = DateTime.UtcNow;
            }
        }
    }
}