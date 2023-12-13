using Microsoft.Xna.Framework;
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
    public class PiercingBind : BaseSkill
    {
        public override int Priority => 1;
        public override int Cooldown => 180;

        public override int UseBugCount => 1;

        public override string SkillName => "PiercingBind";

        public override bool NotWireDash => true;

        public override List<WeaponType> weaponType => new List<WeaponType>() { WeaponType.GreatSword };

        public override bool UseCondition(WireBugPlayer modplayer)
        {
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.active && proj.type == ModContent.ProjectileType<PiercingBindProj>() && proj.owner == modplayer.Player.whoAmI)
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
            int protmp = Projectile.NewProjectile(player.GetSource_Misc("WireBug"), player.Center, ShootVel, ModContent.ProjectileType<PiercingBindProj>(), 1, 5f, player.whoAmI);
            if (protmp >= 0)
            {
                PiercingBindProj modproj = Main.projectile[protmp].ModProjectile as PiercingBindProj;
                modproj.Phase = PiercingBindPhase.Pierce;
                modproj.UsedBugID1 = UseBug1;
                modproj.UsedBugID2 = UseBug2;
                modproj.LockInput = false;
                modproj.DisableMeleeEffect = true;
                return true;
            }
            return false;
        }

    }

    public class PiercingBindGNPC : GlobalNPC
    {
        public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            OnHitEither(Main.player[projectile.owner], npc, hit);
        }
        public override void OnHitByItem(NPC npc, Player player, Item item, NPC.HitInfo hit, int damageDone)
        {
            OnHitEither(player, npc, hit);
        }

        private void OnHitEither(Player player, NPC target, NPC.HitInfo hit)
        {
            int HasTarget = -1;
            Vector2 KnifePos = Vector2.Zero;
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.active && proj.type == ModContent.ProjectileType<PiercingBindProj>() && proj.owner == player.whoAmI)
                {
                    if (proj.ai[2] == 0)
                    {
                        proj.ai[2] = PiercingBindProj.Cooldown;
                        proj.localAI[0] += hit.Damage / 2;
                        HasTarget = (proj.ModProjectile as PiercingBindProj).Target;
                        KnifePos = proj.Center;
                        break;
                    }
                }
            }
            if (HasTarget != -1)
            {
                if (NPCUtils.IsTheSameOwner(Main.npc[HasTarget], target))
                {
                    player.StrikeNPCDirect(target,
                        target.CalculateHitInfo(hit.Damage / 2, hit.HitDirection, false, 0, hit.DamageType, false, player.luck));
                    SlashProj.Summon(player, KnifePos, 0, 0);
                }
            }
        }
    }
}
