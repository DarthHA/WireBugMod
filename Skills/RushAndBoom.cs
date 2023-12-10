using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WireBugMod.Projectiles.Lance;
using WireBugMod.System;
using WireBugMod.System.Skill;
using WireBugMod.Utils;

namespace WireBugMod.Skills
{
    public class RushAndBoom : BaseSkill
    {
        const float MinRange = 200;
        const float MaxRange = 500;


        public override int Priority => 1;
        public override int Cooldown => 180;

        public override int UseBugCount => 1;

        public override string SkillName => "BulletBarrage";

        public override bool NotWireDash => true;

        public override List<WeaponType> weaponType => new List<WeaponType>() { WeaponType.Lance };

        public override bool UseCondition(WireBugPlayer modplayer)
        {
            return modplayer.Player.HeldItem.type == ItemID.NorthPole;
        }
        public override bool OnUse(WireBugPlayer modplayer, int UseBug1, int UseBug2 = -1)
        {
            Player player = modplayer.Player;
            float dist = Main.MouseWorld.Distance(player.Center);
            dist = (float)Math.Clamp(dist, MinRange, MaxRange);
            Vector2 targetPos = player.Center + Vector2.Normalize(Main.MouseWorld - player.Center) * dist;
            targetPos = PlayerUtils.SearchForNotBlockedPos(player.Center, targetPos);

            int protmp = Projectile.NewProjectile(player.GetSource_Misc("WireBug"), modplayer.Player.Center, Vector2.Zero, ModContent.ProjectileType<RushAndBoomProj>(), 0, 0, player.whoAmI);
            if (protmp >= 0)
            {
                RushAndBoomProj modproj = Main.projectile[protmp].ModProjectile as RushAndBoomProj;
                modproj.TargetPos = targetPos;
                modproj.Phase = RushAndBoomPhase.Shoot;
                modproj.StartPos = player.Center;
                modproj.UsedBugID1 = UseBug1;
                modproj.UsedBugID2 = UseBug2;

                return true;
            }
            return false;
        }

    }
}
