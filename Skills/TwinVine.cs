using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using WireBugMod.Projectiles;
using WireBugMod.Projectiles.SBlade;
using WireBugMod.System;
using WireBugMod.System.Skill;
using WireBugMod.Utils;

namespace WireBugMod.Skills
{
    public class TwinVine : BaseSkill
    {
        public override int Priority => 1;
        public override int Cooldown => 180;

        public override int UseBugCount => 1;

        public override string SkillName => "TwinVine";

        public override bool NotWireDash => true;

        public override List<WeaponType> weaponType => new List<WeaponType>() { WeaponType.GreatSword };

        public override bool UseCondition(WireBugPlayer modplayer)
        {
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.active && SkillUtils.CheckSilkbindProj(proj.type) && proj.owner == modplayer.Player.whoAmI)
                {
                    return false;
                }
            }
            return true;
        }
        public override bool OnUse(WireBugPlayer modplayer, int UseBug1, int UseBug2 = -1)
        {
            Player player = modplayer.Player;
            Vector2 ShootVel = Vector2.Normalize(Main.MouseWorld - player.Center);
            int protmp = Projectile.NewProjectile(player.GetSource_Misc("WireBug"), player.Center, ShootVel, ModContent.ProjectileType<TwinVineProj>(), 1, 5f, player.whoAmI);
            if (protmp >= 0)
            {
                player.direction = Math.Sign(ShootVel.X);
                TwinVineProj modproj = Main.projectile[protmp].ModProjectile as TwinVineProj;
                modproj.Phase = TwinVinePhase.Pierce;
                modproj.UsedBugID1 = UseBug1;
                modproj.UsedBugID2 = UseBug2;
                modproj.LockInput = false;
                modproj.DisableMeleeEffect = true;
                modproj.ItemType = player.HeldItem.type;
                modproj.OriginalDir = player.direction;
                return true;
            }
            return false;
        }

    }

}
