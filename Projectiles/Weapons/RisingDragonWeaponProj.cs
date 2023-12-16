using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using WireBugMod.Utils;

namespace WireBugMod.Projectiles.Weapons
{

    public class RisingDragonWeaponProj : BaseWeaponProj
    {
        public override bool IsProjTexture => false;

        public Vector2 OffSet = Vector2.Zero;

        public override string Texture => "WireBugMod/Images/PlaceHolder";


        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            overPlayers.Add(index);
        }

        public override void SafeAI(Player owner)
        {
            Projectile.Center = owner.Center + new Vector2(OffSet.X * owner.direction, OffSet.Y);
            owner.itemLocation = Vector2.Zero;        //ÓÃÀ´±ÜÃâÉÁË¸
            owner.itemTime = owner.itemAnimation = 2;

            /*
            Vector2 vecRot = GetR(Projectile.rotation, owner.direction, Projectile.localAI[0], 1).RotatedBy(Projectile.localAI[1]);
            owner.itemRotation = (float)Math.Atan2(vecRot.Y * owner.direction, vecRot.X * owner.direction) + owner.fullRotation;
            */
        }

        public override void SafeOnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Main.player[Projectile.owner].SetIFrame(120);
        }

        public override bool? SafeColliding(Player owner, Vector2 TexSize, Rectangle targetHitbox)
        {
            float dist = Math.Max(TexSize.X, TexSize.Y) * owner.GetAdjustedItemScale(owner.HeldItem);
            float rot = PlayerUtils.GetRotationByDirection(Projectile.rotation, owner.direction) + owner.fullRotation;
            Vector2 UnitX = (rot + MathHelper.Pi / 4).ToRotationVector2();
            Vector2 UnitY = (rot - MathHelper.Pi / 4).ToRotationVector2();
            float point = 1;

            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center + UnitX * dist * 0.5f, Projectile.Center + UnitX * dist * 0.5f + UnitY * dist, dist, ref point);
        }

        public override void SafeDraw(Player owner, Texture2D texture, ref Color lightColor)
        {
            SpriteEffects spriteEffects = owner.direction >= 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            DrawUtils.DrawSword(texture,
                Projectile.Center - Main.screenPosition,
                Projectile.scale * owner.GetAdjustedItemScale(owner.HeldItem),
                Projectile.rotation + MathHelper.Pi / 4,
                Projectile.localAI[1],
                Projectile.localAI[0],
                spriteEffects);
        }

        public static void SummonSword(Projectile ProjOwner, ref int SwordProj, float rot, float DamageScale = 0, int hitCooldown = 999, string Behavior = "")
        {
            if (SwordProj != -1) Main.projectile[SwordProj].Kill();

            Player owner = Main.player[ProjOwner.owner];

            int protmp = Projectile.NewProjectile(owner.GetSource_ItemUse_WithPotentialAmmo(owner.HeldItem, 0, "WireBug"), owner.Center, Vector2.Zero, ModContent.ProjectileType<RisingDragonWeaponProj>(), owner.GetWeaponDamage(), owner.GetWeaponKnockback(), owner.whoAmI);
            if (protmp >= 0)
            {
                Main.projectile[protmp].rotation = rot;
                Main.projectile[protmp].localNPCHitCooldown = hitCooldown;
                RisingDragonWeaponProj modproj = Main.projectile[protmp].ModProjectile as RisingDragonWeaponProj;
                modproj.ProjOwner = ProjOwner.whoAmI;
                modproj.TexType = owner.HeldItem.type;
                modproj.DamageScale = DamageScale;
                modproj.Behavior = Behavior;
                SwordProj = protmp;
            }
        }

    }
}