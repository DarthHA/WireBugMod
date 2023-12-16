using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using WireBugMod.Projectiles.LSword;
using WireBugMod.Utils;

namespace WireBugMod.Projectiles.Weapons
{
    /// <summary>
    /// 自带方向
    /// </summary>
    public class LSwordWeaponProj : BaseWeaponProj
    {
        public override bool IsProjTexture => false;

        public float[] OldRot = new float[5];

        public bool HasTrail = false;

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            if (Projectile.localAI[0] != MathHelper.Pi / 2 && Projectile.rotation.ToRotationVector2().Y > 0)
            {
                overPlayers.Add(index);
            }
        }
        public override void SafeAI(Player owner)
        {
            //owner.heldProj = Projectile.whoAmI;            //怎么回事呢
            Projectile.Center = owner.Center;
            owner.itemLocation = Vector2.Zero;        //用来避免闪烁
            owner.itemTime = owner.itemAnimation = 2;


            Vector2 vecRot = GetR(Projectile.rotation, owner.direction, Projectile.localAI[0], 1).RotatedBy(Projectile.localAI[1]);
            owner.itemRotation = (float)Math.Atan2(vecRot.Y * owner.direction, vecRot.X * owner.direction) + owner.fullRotation;


            if (!HasTrail)
            {
                for (int i = 0; i < 5; i++)
                {
                    OldRot[i] = Projectile.rotation;
                }
            }
            else
            {
                for (int i = 4; i > 0; i--)
                {
                    OldRot[i] = OldRot[i - 1];
                }
                OldRot[0] = Projectile.rotation;
            }
        }

        public override void SafeOnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Main.player[Projectile.owner].SetIFrame(120);
            if (Behavior == "Sakura")
            {
                if (HitCount <= 2)
                {
                    Vector2 SpawnPos = target.position + new Vector2(Main.rand.Next(target.width), Main.rand.Next(target.height));
                    SlashProj.Summon(Main.player[Projectile.owner], SpawnPos, 0, 0);
                }
                if (HitCount == 1)
                {
                    int protmp = Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, Vector2.Zero, ModContent.ProjectileType<SakuraBombProj>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                    Main.projectile[protmp].ai[0] = target.whoAmI;
                }
            }
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
            if (HasTrail)
            {
                Texture2D blob = ModContent.Request<Texture2D>("WireBugMod/Images/BlobGlow").Value;
                float len = texture.Size().Distance(Vector2.Zero);

                List<CustomVertexInfo> vertexInfos = new();
                for (int i = 0; i < 4; i++)
                {
                    Vector2 rotVec1 = GetR(OldRot[i], owner.direction, Projectile.localAI[0], len + 6);
                    Vector2 rotVec2 = GetR(OldRot[i], owner.direction, Projectile.localAI[0], len - 6);
                    Vector2 rotVec3 = GetR((OldRot[i] + OldRot[i + 1]) / 2f, owner.direction, Projectile.localAI[0], len + 6);
                    Vector2 rotVec4 = GetR((OldRot[i] + OldRot[i + 1]) / 2f, owner.direction, Projectile.localAI[0], len - 6);

                    vertexInfos.Add(new CustomVertexInfo(Projectile.Center + rotVec1, Color.White, new Vector3(1 - i / 4f, 0, 1)));
                    vertexInfos.Add(new CustomVertexInfo(Projectile.Center + rotVec2, Color.White, new Vector3(1 - i / 4f, 1, 1)));
                    vertexInfos.Add(new CustomVertexInfo(Projectile.Center + rotVec3, Color.White, new Vector3(1 - (i + 0.5f) / 4f, 0, 1)));
                    vertexInfos.Add(new CustomVertexInfo(Projectile.Center + rotVec4, Color.White, new Vector3(1 - (i + 0.5f) / 4f, 1, 1)));
                }
                DrawUtils.DrawTrail(blob, vertexInfos, Main.spriteBatch, Color.Cyan, BlendState.Additive);
            }

            SpriteEffects spriteEffects = owner.direction >= 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            DrawUtils.DrawSword(texture,
                Projectile.Center - Main.screenPosition,
                Projectile.scale * owner.GetAdjustedItemScale(owner.HeldItem),
                Projectile.rotation + MathHelper.Pi / 4,
                Projectile.localAI[1],
                Projectile.localAI[0],
                spriteEffects);
        }



        private Vector2 GetR(float rot, int dir, float rotZ, float dist)
        {
            Vector2 result = PlayerUtils.GetRotationByDirection(rot, dir).ToRotationVector2() * dist;
            result.Y *= (float)Math.Sin(rotZ);
            return result;
        }

        public static void SummonSword(Projectile ProjOwner, ref int SwordProj, float rot, float DamageScale = 0, int hitCooldown = 999, string Behavior = "")
        {
            if (SwordProj != -1) Main.projectile[SwordProj].Kill();

            Player owner = Main.player[ProjOwner.owner];

            int protmp = Projectile.NewProjectile(owner.GetSource_ItemUse_WithPotentialAmmo(owner.HeldItem, 0, "WireBug"), owner.Center, Vector2.Zero, ModContent.ProjectileType<LSwordWeaponProj>(), owner.GetWeaponDamage(), owner.GetWeaponKnockback(), owner.whoAmI);
            if (protmp >= 0)
            {
                Main.projectile[protmp].rotation = rot;
                Main.projectile[protmp].localNPCHitCooldown = hitCooldown;
                LSwordWeaponProj modproj = Main.projectile[protmp].ModProjectile as LSwordWeaponProj;
                modproj.ProjOwner = ProjOwner.whoAmI;
                modproj.TexType = owner.HeldItem.type;
                modproj.DamageScale = DamageScale;
                modproj.Behavior = Behavior;
                SwordProj = protmp;
            }
        }

    }
}