using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using WireBugMod.Utils;

namespace WireBugMod.Projectiles.GSword
{
    public class GSwordWeaponProj : ModProjectile
    {
        public int ItemType = -1;

        public int ProjOwner = -1;

        public int Behavior = 0;

        public bool Hit = false;
        public override string Texture => "WireBugMod/Images/PlaceHolder";
        public override void SetStaticDefaults()
        {

        }
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.timeLeft = 99999;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.netImportant = true;

            Projectile.friendly = true;
            Projectile.damage = 1;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 9999;
            Projectile.ownerHitCheck = true;
        }
        public override void AI()
        {
            if (ItemType == -1)
            {
                Projectile.Kill();
                return;
            }

            if (ProjOwner == -1 || !Main.projectile[ProjOwner].active)
            {
                Projectile.Kill();
                return;
            }

            Player owner = Main.player[Projectile.owner];
            if (owner.IsDead())
            {
                Projectile.Kill();
                return;
            }
            owner.heldProj = Projectile.whoAmI;
            Projectile.Center = owner.Center;
            owner.itemLocation = Vector2.Zero;
            owner.itemTime = owner.itemAnimation = 2;


            float rotdir = PlayerUtils.GetRotationByDirection(Projectile.rotation, owner.direction);
            owner.ChangeItemRotation(rotdir);

        }


        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Main.player[Projectile.owner].SetIFrame(120);
            Hit = true;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Player owner = Main.player[Projectile.owner];

            Texture2D tex = DrawUtils.GetItemTexture(ItemType);
            float dist = Math.Max(tex.Width, tex.Height) * owner.GetAdjustedItemScale(owner.HeldItem);
            float rot = PlayerUtils.GetRotationByDirection(Projectile.rotation, owner.direction) + owner.fullRotation;
            Vector2 UnitX = (rot + MathHelper.Pi / 4).ToRotationVector2();
            Vector2 UnitY = (rot - MathHelper.Pi / 4).ToRotationVector2();
            float point = 1;

            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center + UnitX * dist * 0.5f, Projectile.Center + UnitX * dist * 0.5f + UnitY * dist, dist, ref point);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Player owner = Main.player[Projectile.owner];

            //float rotdir = GetRotByDir(Projectile.rotation, owner.direction);
            //Terraria.Utils.DrawLine(Main.spriteBatch, Projectile.Center, Projectile.Center + rotdir.ToRotationVector2() * 200, Color.Wheat);
            //Main.spriteBatch.Draw(Terraria.GameContent.TextureAssets.MagicPixel.Value, new Rectangle((int)(owner.position.X-Main.screenPosition.X), (int)(owner.position.Y-Main.screenPosition.Y), owner.width, owner.height), Color.Pink * 0.5f);

            Texture2D tex = DrawUtils.GetItemTexture(ItemType);

            Vector2 origin = new Vector2(owner.direction >= 0 ? 0 : tex.Size().X, tex.Size().Y);

            SpriteEffects spriteEffects = owner.direction >= 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            float rot = PlayerUtils.GetRotationByDirection(Projectile.rotation, owner.direction) + owner.fullRotation;
            rot += owner.direction >= 0 ? MathHelper.Pi / 4 : MathHelper.Pi / 4 * 3;
            Main.spriteBatch.Draw(tex,
                Projectile.Center - Main.screenPosition,
                null,
                lightColor * (1 - owner.immuneAlpha / 255f),
                rot,
                origin,
                Projectile.scale * owner.GetAdjustedItemScale(owner.HeldItem),
                spriteEffects,
                0);

            return false;
        }

    }
}