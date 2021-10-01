using HarmonyLib;
using BetterTownOfUs.Roles;

namespace BetterTownOfUs.ImpostorRoles.AssassinMod
{
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.VotingComplete))] // BBFDNCCEJHI
    public static class VotingComplete
    {
        public static void Postfix(MeetingHud __instance)
        {
            if (PlayerControl.LocalPlayer.Is(RoleEnum.Assassin))
            {
                var assassin = Role.GetRole<Assassin>(PlayerControl.LocalPlayer);
                ShowHideButtons.HideButtons(assassin);
            }
        }
    }
}