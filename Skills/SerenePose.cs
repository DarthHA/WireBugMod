using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using WireBugMod.Projectiles.LSword;
using WireBugMod.System;
using WireBugMod.System.Skill;

namespace WireBugMod.Skills
{
    public class SerenePose : BaseSkill
    {
        public override int Priority => 1;
        public override int Cooldown => 180;


        public override int UseBugCount => 2;

        public override string SkillName => "SerenePose";

        public override bool NotWireDash => true;



        public override List<WeaponType> weaponType => new List<WeaponType>() { WeaponType.GreatSword };
        public override bool UseCondition(WireBugPlayer modplayer)
        {
            Player player = modplayer.Player;
            if (player.velocity.Y == 0)
            {
                return true;
            }
            return false;
        }
        public override bool OnUse(WireBugPlayer modplayer, int UseBug1, int UseBug2 = -1)
        {
            Player player = modplayer.Player;

            int protmp = Projectile.NewProjectile(player.GetSource_Misc("WireBug"), modplayer.Player.Center, Vector2.Zero, ModContent.ProjectileType<SernePoseProj>(), 0, 0, player.whoAmI);
            if (protmp >= 0)
            {
                SernePoseProj modproj = Main.projectile[protmp].ModProjectile as SernePoseProj;
                modproj.Phase = SernePosePhase.Guard;
                modproj.ActivatingGP = true;
                modproj.ShieldLevel = 2;
                modproj.DisableMeleeEffect = true;
                modproj.UsedBugID1 = UseBug1;
                modproj.UsedBugID2 = UseBug2;

                return true;
            }

            return false;
        }

    }
}
