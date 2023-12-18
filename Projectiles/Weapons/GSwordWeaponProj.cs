using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using WireBugMod.Utils;

namespace WireBugMod.Projectiles.Weapons
{
    public class GSwordWeaponProj : BaseWeaponProj
    {
        public override bool IsProjTexture => false;


        public override void SafeAI(Player owner)
        {
            owner.heldProj = Projectile.whoAmI;
            Projectile.Center = owner.Center;
            owner.itemLocation = Vector2.Zero;
            owner.itemTime = owner.itemAnimation = 2;

            float rotdir = PlayerUtils.GetRotationByDirection(Projectile.rotation, owner.direction);
            owner.ChangeItemRotation(rotdir);
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

        public override void SafeOnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Behavior == "ACSlash")
            {
                (Main.projectile[ProjOwner].ModProjectile as BaseSkillProj).SleepTimer = 10;
            }
            else if (Behavior == "SlashDown")
            {
                (Main.projectile[ProjOwner].ModProjectile as BaseSkillProj).SleepTimer = 10;
            }
        }

        public override void SafeDraw(Player owner, Texture2D texture, ref Color lightColor)
        {
            //float rotdir = GetRotByDir(Projectile.rotation, owner.direction);
            //Terraria.Utils.DrawLine(Main.spriteBatch, Projectile.Center, Projectile.Center + rotdir.ToRotationVector2() * 200, Color.Wheat);
            //Main.spriteBatch.Draw(Terraria.GameContent.TextureAssets.MagicPixel.Value, new Rectangle((int)(owner.position.X-Main.screenPosition.X), (int)(owner.position.Y-Main.screenPosition.Y), owner.width, owner.height), Color.Pink * 0.5f);


            Vector2 origin = new Vector2(owner.direction >= 0 ? 0 : texture.Size().X, texture.Size().Y);

            SpriteEffects spriteEffects = owner.direction >= 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            float rot = PlayerUtils.GetRotationByDirection(Projectile.rotation, owner.direction) + owner.fullRotation;
            rot += owner.direction >= 0 ? MathHelper.Pi / 4 : MathHelper.Pi / 4 * 3;
            Main.spriteBatch.Draw(texture,
                Projectile.Center - Main.screenPosition,
                null,
                lightColor * (1 - owner.immuneAlpha / 255f),
                rot,
                origin,
                Projectile.scale * owner.GetAdjustedItemScale(owner.HeldItem),
                spriteEffects,
                0);

        }


        public static void SummonSword(Projectile ProjOwner, ref int SwordProj, float rot, float DamageScale = 0, int hitCooldown = 999, string Behavior = "")
        {
            if (SwordProj != -1) Main.projectile[SwordProj].Kill();

            Player owner = Main.player[ProjOwner.owner];

            int protmp = Projectile.NewProjectile(owner.GetSource_ItemUse_WithPotentialAmmo(owner.HeldItem, 0, "WireBug"), owner.Center, Vector2.Zero, ModContent.ProjectileType<GSwordWeaponProj>(), owner.GetWeaponDamage(), owner.GetWeaponKnockback(), owner.whoAmI);
            if (protmp >= 0)
            {
                Main.projectile[protmp].rotation = rot;
                Main.projectile[protmp].localNPCHitCooldown = hitCooldown;
                GSwordWeaponProj modproj = Main.projectile[protmp].ModProjectile as GSwordWeaponProj;
                modproj.ProjOwner = ProjOwner.whoAmI;
                modproj.TexType = owner.HeldItem.type;
                modproj.DamageScale = DamageScale;
                modproj.Behavior = Behavior;
                SwordProj = protmp;
            }
        }
    }
}