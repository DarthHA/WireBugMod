using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using WireBugMod.Projectiles.GSword;
using WireBugMod.System;
using WireBugMod.System.Skill;
using WireBugMod.Utils;


namespace WireBugMod.Skills
{
    /*
    public class SoaringKick : BaseSkill
    {
        const float MaxRange = 800;

        const float MinRange = 300;

        const float UpwardDist = 200;
        public override int Priority => 2;
        public override int Cooldown => 180;

        public override int UseBugCount => 1;

        public override string SkillName => "SoaringKick";

        public override bool NotWireDash => true;

        public override List<WeaponType> weaponType => new List<WeaponType>() { WeaponType.GreatSword };

        public override bool OnUse(WireBugPlayer modplayer, int UseBug1, int UseBug2 = -1)
        {
            Player player = modplayer.Player;
            float TargetX = Main.MouseWorld.X;

            if (Math.Abs(TargetX - player.Center.X) < MinRange)
            {
                TargetX = player.Center.X + MinRange * Math.Sign(TargetX - player.Center.X);
            }
            else if (Math.Abs(TargetX - player.Center.X) > MaxRange)
            {
                TargetX = player.Center.X + MaxRange * Math.Sign(TargetX - player.Center.X);
            }

            //TargetX = Math.Clamp(TargetX, player.Center.X - MaxRange, player.Center.X + MaxRange);
            if (TargetX == player.Center.X) TargetX += 1;
            float TargetDistX = TargetX - player.Center.X;
            float RealX = Math.Abs(TargetDistX), RealY = Math.Clamp(RealX * 0.5f, UpwardDist, 114514);
            bool findPos = false;
            for (; RealY > RealX * 0.3f; RealY -= 16)
            {
                Vector2 UpPos = player.Center + new Vector2(Math.Sign(TargetDistX) * 0.75f * RealX, -RealY);

                Vector2 DownPos = player.Center + new Vector2(Math.Sign(TargetDistX) * RealX, 0);
                Vector2 Unit = DownPos - UpPos;
                DownPos = PlayerUtils.SearchForNotBlockedPos(UpPos, UpPos + Unit * 1.5f);

                if (Collision.CanHit(player.position, player.width, player.height, UpPos, 1, 1) && DownPos.Distance(UpPos) > 50)
                {
                    findPos = true;
                    break;
                }
            }


            if (findPos)
            {
                Vector2 targetPos = player.Center + new Vector2(Math.Sign(TargetDistX) * 0.75f * RealX, -RealY);
                Vector2 downPos = player.Center + new Vector2(Math.Sign(TargetDistX) * RealX, 0);
                Vector2 Unit = downPos - targetPos;
                downPos = PlayerUtils.SearchForNotBlockedPos(targetPos, targetPos + Unit * 1.5f);


                int protmp = Projectile.NewProjectile(player.GetSource_Misc("WireBug"), modplayer.Player.Center, Vector2.Zero, ModContent.ProjectileType<SlashDownProj>(), 0, 0, player.whoAmI);
                if (protmp >= 0)
                {
                    SlashDownProj modproj = Main.projectile[protmp].ModProjectile as SlashDownProj;
                    modproj.TargetPos = targetPos;
                    modproj.Phase = SlashDownPhase.Shoot1;
                    modproj.StartPos = player.Center;
                    modproj.DownPos = downPos;
                    modproj.DisableMeleeEffect = true;
                    modproj.UsedBugID1 = UseBug1;
                    modproj.UsedBugID2 = UseBug2;

                    player.GetModPlayer<MiscEffectPlayer>().DisableMeleeEffect = true;

                    return true;
                }
            }
            return false;

        }
    }
    */
}
