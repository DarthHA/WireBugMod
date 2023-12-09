using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using WireBugMod.Utils;

namespace WireBugMod.Projectiles.SBlade
{

    public class RisingDragonWeaponProj : ModProjectile
    {
        public int ItemType = -1;

        public int ProjOwner = -1;

        public bool Hit = false;

        public Vector2 OffSet = Vector2.Zero;

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

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            overPlayers.Add(index);
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

            Projectile.Center = owner.Center + new Vector2(OffSet.X * owner.direction, OffSet.Y);
            owner.itemLocation = Vector2.Zero;        //ÓÃÀ´±ÜÃâÉÁË¸
            owner.itemTime = owner.itemAnimation = 2;

            /*
            Vector2 vecRot = GetR(Projectile.rotation, owner.direction, Projectile.localAI[0], 1).RotatedBy(Projectile.localAI[1]);
            owner.itemRotation = (float)Math.Atan2(vecRot.Y * owner.direction, vecRot.X * owner.direction) + owner.fullRotation;
            */
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


            Texture2D tex = DrawUtils.GetItemTexture(ItemType);

            SpriteEffects spriteEffects = owner.direction >= 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            DrawUtils.DrawSword(tex,
                Projectile.Center - Main.screenPosition,
                Projectile.scale * owner.GetAdjustedItemScale(owner.HeldItem),
                Projectile.rotation + MathHelper.Pi / 4,
                Projectile.localAI[1],
                Projectile.localAI[0],
                spriteEffects);
            return false;
        }

    }
}