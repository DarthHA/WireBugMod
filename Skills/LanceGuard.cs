using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using WireBugMod.Projectiles.Lance;
using WireBugMod.System;
using WireBugMod.System.Skill;

namespace WireBugMod.Skills
{
    public class LanceGuard : BaseSkill
    {

        public override int Priority => 2;
        public override int Cooldown => 180;

        public override int UseBugCount => 1;

        public override string SkillName => "AnchorRage";

        public override bool NotWireDash => true;

        public override List<WeaponType> weaponType => new List<WeaponType>() { WeaponType.Lance };
        public override bool UseCondition(WireBugPlayer modplayer)
        {
            Player player = modplayer.Player;
            if (player.hasRaisableShield && player.velocity.Y == 0)
            {
                return true;
            }
            return false;
        }
        public override bool OnUse(WireBugPlayer modplayer, int UseBug1, int UseBug2 = -1)
        {
            Player player = modplayer.Player;
            player.direction = Math.Sign(Main.MouseWorld.X - player.Center.X);
            int protmp = Projectile.NewProjectile(player.GetSource_Misc("WireBug"), modplayer.Player.Center, Vector2.Zero, ModContent.ProjectileType<LanceGuardProj>(), 0, 0, player.whoAmI);
            if (protmp >= 0)
            {
                LanceGuardProj modproj = Main.projectile[protmp].ModProjectile as LanceGuardProj;
                modproj.Phase = LanceGuardPhase.Guard;
                modproj.ShieldRaise = true;
                modproj.ActivatingGP = true;
                modproj.ShieldLevel = 2;
                modproj.UsedBugID1 = UseBug1;
                modproj.UsedBugID2 = UseBug2;

                return true;
            }
            return false;

        }

    }
}
