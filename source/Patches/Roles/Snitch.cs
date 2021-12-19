using System.Collections.Generic;
using BetterTownOfUs.ImpostorRoles.CamouflageMod;
using UnityEngine;

namespace BetterTownOfUs.Roles
{
    public class Snitch : Role
    {
        public List<ArrowBehaviour> ImpArrows = new List<ArrowBehaviour>();

        public List<ArrowBehaviour> SnitchArrows = new List<ArrowBehaviour>();

        public List<PlayerControl> SnitchTargets = new List<PlayerControl>();

        public int TasksLeft = int.MaxValue;

        public Snitch(PlayerControl player) : base(player, RoleEnum.Snitch)
        {
            ImpostorText = () => "Complete all your tasks to discover the Impostors";
            TaskText = () =>
                TasksDone
                    ? "Find the arrows pointing to the Impostors!"
                    : "Complete all your tasks to discover the Impostors!";
            Hidden = !CustomGameOptions.SnitchOnLaunch;
        }

        public bool Revealed => TasksLeft <= CustomGameOptions.SnitchTasksRemaining;
        public bool TasksDone => TasksLeft <= 0;


        internal override bool Criteria()
        {
            return Revealed && PlayerControl.LocalPlayer.Is(Faction.Impostors) ||
                   base.Criteria();
        }

        protected override string NameText(PlayerVoteArea player = null)
        {
            if (CamouflageUnCamouflage.IsCamoed && player == null) return "";
            if (PlayerControl.LocalPlayer.Data.IsDead) return base.NameText(player);
            if (Revealed || !Hidden) return base.NameText(player);
            Player.nameText.color = Color.white;
            if (player != null) player.NameText.color = Color.white;
            if (player != null && (MeetingHud.Instance.state == MeetingHud.VoteStates.Proceeding ||
                                   MeetingHud.Instance.state == MeetingHud.VoteStates.Results)) return Player.name;
            if (!CustomGameOptions.RoleUnderName && player == null) return Player.name;
            Player.nameText.transform.localPosition = new Vector3(
                0f,
                Player.CurrentOutfit.HatId == "hat_NoHat" ? 1.5f : 2.0f,
                -0.5f
            );
            return Player.name + "\n" + "Crewmate";
        }
    }
}
