using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;
using WireBugMod.Projectiles.Lance;
using WireBugMod.System;
using WireBugMod.System.Skill;
using WireBugMod.Utils;

namespace WireBugMod.Skills
{
    public class LanceDown : BaseSkill
    {
        const float MaxRange = 300;

        const float UpwardDist = 400;
        public override int Priority => 2;
        public override int Cooldown => 180;

        public override int UseBugCount => 1;

        public override string SkillName => "SkywardThrust";

        public override bool NotWireDash => true;


        public override WeaponType weaponType => WeaponType.Lance;
        public override bool OnUse(WireBugPlayer modplayer, int UseBug1, int UseBug2 = -1)
        {
            Player player = modplayer.Player;
            float TargetX = Main.MouseWorld.X;
            TargetX = Math.Clamp(TargetX, player.Center.X - MaxRange, player.Center.X + MaxRange);
            if (TargetX == player.Center.X) TargetX += 1;
            float TargetDistX = TargetX - player.Center.X;
            float RealX = Math.Abs(TargetDistX), RealY = UpwardDist;
            bool findPos = false;
            for (; RealY > UpwardDist / 2; RealY -= 16)
            {
                for (RealX = Math.Abs(TargetDistX); RealX > 0; RealX -= 16)
                {
                    Vector2 UpPos = player.Center + new Vector2(Math.Sign(TargetDistX) * RealX, -RealY);
                    if (Collision.CanHit(player.position, player.width, player.height, UpPos, 1, 1))
                    {
                        findPos = true;
                        break;
                    }
                }
                if (findPos) break;
            }


            if (findPos)
            {
                Vector2 targetPos = player.Center + new Vector2(Math.Sign(TargetDistX) * RealX, -RealY);
                Vector2 downPos = PlayerUtils.SearchForNotBlockedPos(targetPos, targetPos + new Vector2(0, 1200));

                int protmp = Projectile.NewProjectile(player.GetSource_Misc("WireBug"), modplayer.Player.Center, Vector2.Zero, ModContent.ProjectileType<LanceDownProj>(), 0, 0, player.whoAmI);
                if (protmp >= 0)
                {
                    LanceDownProj modproj = Main.projectile[protmp].ModProjectile as LanceDownProj;
                    modproj.TargetPos = targetPos;
                    modproj.Phase = LanceDownPhase.Shoot1;
                    modproj.StartPos = player.Center;
                    modproj.DownPos = downPos;
                    modproj.UsedBugID1 = UseBug1;
                    modproj.UsedBugID2 = UseBug2;

                    return true;
                }
            }
            return false;

        }

    }
}
