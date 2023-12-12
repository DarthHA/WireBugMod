using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using WireBugMod.Utils;

namespace WireBugMod.Projectiles.LSword
{

    public class LSwordWeaponProj : ModProjectile
    {
        public int ItemType = -1;

        public int ProjOwner = -1;

        public bool HasTrail = false;

        public bool Hit = false;

        public bool Sakura = false;

        private int HitNumber = 0;

        public float[] OldRot = new float[5];
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
            if (Projectile.localAI[0] != MathHelper.Pi / 2 && Projectile.rotation.ToRotationVector2().Y > 0)
            {
                overPlayers.Add(index);
            }
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


        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Main.player[Projectile.owner].SetIFrame(120);
            if (Sakura)
            {
                HitNumber++;
                if (HitNumber <= 2)
                {
                    Vector2 SpawnPos = target.position + new Vector2(Main.rand.Next(target.width), Main.rand.Next(target.height));
                    SlashProj.Summon(Main.player[Projectile.owner], SpawnPos, 0, 0);
                }
                if (!Hit)
                {
                    int protmp = Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, Vector2.Zero, ModContent.ProjectileType<SakuraBombProj>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                    Main.projectile[protmp].ai[0] = target.whoAmI;
                }
            }
            Hit = true;


        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Player owner = Main.player[Projectile.owner];

            Texture2D tex = DrawUtils.GetItemTexture(ItemType);
            float dist = Math.Max(tex.Width, tex.Height) * owner.GetAdjustedItemScale(owner.HeldItem);
            float rot = GetRotByDir(Projectile.rotation, owner.direction) + owner.fullRotation;
            Vector2 UnitX = (rot + MathHelper.Pi / 4).ToRotationVector2();
            Vector2 UnitY = (rot - MathHelper.Pi / 4).ToRotationVector2();
            float point = 1;

            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center + UnitX * dist * 0.5f, Projectile.Center + UnitX * dist * 0.5f + UnitY * dist, dist, ref point);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Player owner = Main.player[Projectile.owner];


            Texture2D tex = DrawUtils.GetItemTexture(ItemType);

            if (HasTrail)
            {
                Texture2D blob = ModContent.Request<Texture2D>("WireBugMod/Images/BlobGlow").Value;
                float len = tex.Size().Distance(Vector2.Zero);

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

            DrawUtils.DrawSword(tex,
                Projectile.Center - Main.screenPosition,
                Projectile.scale * owner.GetAdjustedItemScale(owner.HeldItem),
                Projectile.rotation + MathHelper.Pi / 4,
                Projectile.localAI[1],
                Projectile.localAI[0],
                spriteEffects);
            return false;
        }


        private float GetRotByDir(float rot, int dir)
        {
            if (dir > 0)
            {
                return rot;
            }
            else
            {
                Vector2 temp = rot.ToRotationVector2();
                temp.X = -temp.X;
                rot = temp.ToRotation();
                return rot;
            }
        }

        private Vector2 GetR(float rot, int dir, float rotZ, float dist)
        {
            Vector2 result = GetRotByDir(rot, dir).ToRotationVector2() * dist;
            result.Y *= (float)Math.Sin(rotZ);
            return result;
        }
    }
}