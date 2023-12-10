using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using WireBugMod.Projectiles;
using WireBugMod.System;
using WireBugMod.System.Skill;

namespace WireBugMod.Skills
{
    public class ReverseBlast : BaseSkill
    {
        public override int Priority => 1;
        public override int Cooldown => 180;

        public override int UseBugCount => 1;

        public override string SkillName => "ReverseBlast";

        public override bool NotWireDash => true;

        public override List<WeaponType> weaponType => new List<WeaponType>() { WeaponType.Lance };
        public override bool OnUse(WireBugPlayer modplayer, int UseBug1, int UseBug2 = -1)
        {
            Player player = modplayer.Player;
            //targetPos = PlayerUtils.SearchForNotBlockedPos(player.Center, targetPos);
            float MoveRotation = (Main.MouseWorld - player.Center).ToRotation();
            int protmp = Projectile.NewProjectile(player.GetSource_Misc("WireBug"), player.Center, Vector2.Zero, ModContent.ProjectileType<ReverseBlastProj>(), 0, 0, player.whoAmI);
            if (protmp >= 0)
            {
                ReverseBlastProj modproj = Main.projectile[protmp].ModProjectile as ReverseBlastProj;
                modproj.Phase = ReverseBlastPhase.PrePare;
                modproj.MovingRotation = MoveRotation;
                modproj.UsedBugID1 = UseBug1;
                modproj.UsedBugID2 = UseBug2;

                return true;
            }
            return false;
        }

    }
}
