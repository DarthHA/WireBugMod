using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;
using WireBugMod.Projectiles;
using WireBugMod.System;
using WireBugMod.System.Skill;
using WireBugMod.Utils;

namespace WireBugMod.Skills
{
    public class WireDash : BaseSkill
    {
        const float MinRange = 100;
        const float MaxRange = 700;

        const int ImmumeFrame = 60 * 2;

        public override int Priority => 1;
        public override int Cooldown => 180;

        public override int UseBugCount => 1;

        public override string SkillName => "WireDash";

        public override bool NotWireDash => false;



        public override WeaponType weaponType => WeaponType.None;

        public override bool OnUse(WireBugPlayer modplayer, int UseBug1, int UseBug2 = -1)
        {
            Player player = modplayer.Player;
            float dist = Main.MouseWorld.Distance(player.Center);
            dist = (float)Math.Clamp(dist, MinRange, MaxRange);
            Vector2 targetPos = player.Center + Vector2.Normalize(Main.MouseWorld - player.Center) * dist;
            targetPos = PlayerUtils.SearchForNotBlockedPos(player.Center, targetPos);

            int protmp = Projectile.NewProjectile(player.GetSource_Misc("WireBug"), modplayer.Player.Center, Vector2.Zero, ModContent.ProjectileType<WireDashProj>(), 0, 0, player.whoAmI);
            if (protmp >= 0)
            {
                WireDashProj modproj = Main.projectile[protmp].ModProjectile as WireDashProj;
                modproj.TargetPos = targetPos;
                modproj.Phase = WireDashPhase.Shoot;
                modproj.StartPos = player.Center;
                modproj.UsedBugID1 = UseBug1;
                modproj.UsedBugID2 = UseBug2;
                if (player.immune)
                {
                    player.SetIFrame(ImmumeFrame);
                }
                return true;
            }
            return false;
        }

    }
}
