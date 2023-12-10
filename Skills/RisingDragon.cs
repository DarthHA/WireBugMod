using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using WireBugMod.Projectiles.SBlade;
using WireBugMod.System;
using WireBugMod.System.Skill;
using WireBugMod.Utils;

namespace WireBugMod.Skills
{
    public class RisingDragon : BaseSkill
    {

        const float UpwardDist = 400;

        const float MinUpDist = 250;
        public override int Priority => 2;
        public override int Cooldown => 180;

        public override int UseBugCount => 1;

        public override string SkillName => "MetsuShoryugeki";

        public override bool NotWireDash => true;


        public override List<WeaponType> weaponType => new List<WeaponType>() { WeaponType.GreatSword };

        public override bool UseCondition(WireBugPlayer modplayer)
        {
            return modplayer.Player.hasRaisableShield;
        }
        public override bool OnUse(WireBugPlayer modplayer, int UseBug1, int UseBug2 = -1)
        {
            Player player = modplayer.Player;
            Vector2 targetPos = PlayerUtils.SearchForNotBlockedPos(player.Center, player.Center + new Vector2(0, -UpwardDist));


            if (targetPos.Distance(player.Center) > MinUpDist)
            {
                Vector2 downPos = PlayerUtils.SearchForNotBlockedPos(player.Center, player.Center + new Vector2(0, UpwardDist / 2f));

                int protmp = Projectile.NewProjectile(player.GetSource_Misc("WireBug"), modplayer.Player.Center, Vector2.Zero, ModContent.ProjectileType<RisingDragonProj>(), 0, 0, player.whoAmI);
                if (protmp >= 0)
                {
                    RisingDragonProj modproj = Main.projectile[protmp].ModProjectile as RisingDragonProj;
                    modproj.DisableMeleeEffect = true;
                    modproj.ShieldRaise = true;
                    modproj.ActivatingGP = true;
                    modproj.ShieldLevel = 2;
                    modproj.OwnerDefaultDirection = player.direction;
                    modproj.TargetPos = targetPos;
                    modproj.Phase = RisingDragonPhase.Shoot;
                    modproj.StartPos = player.Center;
                    modproj.DownPos = downPos;
                    modproj.UsedBugID1 = UseBug1;
                    modproj.UsedBugID2 = UseBug2;
                    player.velocity.X = 0;
                    return true;
                }
            }
            return false;

        }

    }
}
