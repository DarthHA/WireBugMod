using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using WireBugMod.Projectiles.GSword;
using WireBugMod.Projectiles.Lance;
using WireBugMod.System;
using WireBugMod.System.Skill;

namespace WireBugMod.Skills
{
    public class ACSlash : BaseSkill
    {
        const float MinRange = 200;
        const float MaxRange = 500;


        public override int Priority => 1;
        public override int Cooldown => 180;

        public override int UseBugCount => 1;

        public override string SkillName => "AdamantChargedSlash";

        public override bool NotWireDash => true;


        public override List<WeaponType> weaponType => new List<WeaponType>() { WeaponType.GreatSword };

        public override bool OnUse(WireBugPlayer modplayer, int UseBug1, int UseBug2 = -1)
        {
            Player player = modplayer.Player;
            Vector2 targetVec = Main.MouseWorld - player.Center;
            if (targetVec.Y != 0 && Math.Abs(targetVec.Y / targetVec.X) > 0.15f)
            {
                targetVec.Y = Math.Abs(0.15f * targetVec.X) * Math.Sign(targetVec.Y);
            }
            float dist = targetVec.Length();
            dist = (float)Math.Clamp(dist, MinRange, MaxRange);
            targetVec = player.Center + Vector2.Normalize(targetVec) * dist;

            int protmp = Projectile.NewProjectile(player.GetSource_Misc("WireBug"), modplayer.Player.Center, Vector2.Zero, ModContent.ProjectileType<ACSlashProj>(), 0, 0, player.whoAmI);
            if (protmp >= 0)
            {
                ACSlashProj modproj = Main.projectile[protmp].ModProjectile as ACSlashProj;
                modproj.TargetPos = targetVec;
                modproj.Phase = ACSlashPhase.Shoot;
                modproj.StartPos = player.Center;
                modproj.DisableMeleeEffect = true;
                modproj.ShieldLevel = 2;
                modproj.UsedBugID1 = UseBug1;
                modproj.UsedBugID2 = UseBug2;

                return true;
            }
            return false;
        }

    }
}
