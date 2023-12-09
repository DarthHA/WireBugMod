using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;
using WireBugMod.Projectiles.LSword;
using WireBugMod.System;
using WireBugMod.System.Skill;

namespace WireBugMod.Skills
{
    public class SakuraSlash : BaseSkill
    {
        const float MinRange = 500;
        const float MaxRange = 501;
        public override int Priority => 1;
        public override int Cooldown => 180;

        public override int UseBugCount => 1;

        public override string SkillName => "SakuraSlash";

        public override bool NotWireDash => true;


        public override WeaponType weaponType => WeaponType.GreatSword;
        public override bool OnUse(WireBugPlayer modplayer, int UseBug1, int UseBug2 = -1)
        {
            Player player = modplayer.Player;
            float dist = Main.MouseWorld.Distance(player.Center);
            dist = (float)Math.Clamp(dist, MinRange, MaxRange);
            Vector2 targetPos = player.Center + Vector2.Normalize(Main.MouseWorld - player.Center) * dist;
            //targetPos = PlayerUtils.SearchForNotBlockedPos(player.Center, targetPos);

            int protmp = Projectile.NewProjectile(player.GetSource_Misc("WireBug"), modplayer.Player.Center, Vector2.Zero, ModContent.ProjectileType<SakuraSlashProj>(), 0, 0, player.whoAmI);
            if (protmp >= 0)
            {
                SakuraSlashProj modproj = Main.projectile[protmp].ModProjectile as SakuraSlashProj;
                modproj.TargetPos = targetPos;
                modproj.Phase = SakuraSlashPhase.Shoot;
                modproj.StartPos = player.Center;
                modproj.DisableMeleeEffect = true;
                modproj.UsedBugID1 = UseBug1;
                modproj.UsedBugID2 = UseBug2;

                return true;
            }
            return false;
        }

    }
}
