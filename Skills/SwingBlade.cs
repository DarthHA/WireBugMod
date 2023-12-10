using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using WireBugMod.Projectiles.SBlade;
using WireBugMod.System;
using WireBugMod.System.Skill;

namespace WireBugMod.Skills
{
    public class SwingBlade : BaseSkill
    {
        public override int Priority => 1;
        public override int Cooldown => 120;

        public override int UseBugCount => 1;

        public override string SkillName => "Windmill";

        public override bool NotWireDash => true;


        public override List<WeaponType> weaponType => new List<WeaponType>() { WeaponType.GreatSword };

        public override bool OnUse(WireBugPlayer modplayer, int UseBug1, int UseBug2 = -1)
        {
            Player player = modplayer.Player;
            player.direction = Math.Sign(Main.MouseWorld.X - player.Center.X);
            int protmp = Projectile.NewProjectile(player.GetSource_Misc("WireBug"), modplayer.Player.Center, Vector2.Zero, ModContent.ProjectileType<SwingBladeProj>(), 0, 0, player.whoAmI);
            if (protmp >= 0)
            {
                SwingBladeProj modproj = Main.projectile[protmp].ModProjectile as SwingBladeProj;
                modproj.Phase = SwingBladePhase.Begin;
                modproj.ShieldLevel = 3;
                modproj.DisableMeleeEffect = true;
                modproj.UsedBugID1 = UseBug1;
                modproj.UsedBugID2 = UseBug2;

                return true;
            }

            return false;
        }

    }
}
