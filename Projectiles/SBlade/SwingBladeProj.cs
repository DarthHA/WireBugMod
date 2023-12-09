using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WireBugMod.System;
using WireBugMod.Utils;

namespace WireBugMod.Projectiles.SBlade
{
    public enum SwingBladePhase
    {
        Begin,
        Swing,
        End,
        Default
    }

    public class SwingBladeProj : BaseSkillProj        //特殊：这个弹幕会造成伤害
    {
        public override string Texture => "WireBugMod/Images/PlaceHolder";

        public SwingBladePhase Phase = SwingBladePhase.Default;

        private float StringLen = 0;
        //private Vector2 DrawCenter = Vector2.Zero;

        /*
        const int TrailCount = 15;
        public float[] OldRot = new float[TrailCount];
        public float[] OldRadian = new float[TrailCount];
        public Vector2[] OldCenter = new Vector2[TrailCount];
        private bool HasTrail = false;
        */
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
            Projectile.DamageType = DamageClass.Melee;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (owner.IsDead() || owner.HeldItem.GetWeaponType() != WeaponType.ShortBlade)
            {
                Projectile.Kill();
                return;
            }

            if (Main.rand.NextBool(12))
            {
                for (int i = 0; i < 2; i++)
                {
                    GenDust(Projectile.Center + new Vector2(Main.rand.Next(-20, 20), Main.rand.Next(-20, 20)), Main.rand.Next(5), 1 + Main.rand.NextFloat() * 0.5f);
                }
            }
            /*
            开始旋转
            伸出铁虫丝
            长度最大时加速变为残影
            最后一圈半圈减速半圈加速
            转完后收回
            */
            Lighting.AddLight((int)Projectile.Center.X / 16, (int)Projectile.Center.Y / 16, 1.5f, 1.5f, 1.5f);

            owner.velocity.X = 0;

            //owner.heldProj = Projectile.whoAmI;
            Projectile.Center = owner.Center;
            owner.itemLocation = Vector2.Zero;
            owner.itemTime = owner.itemAnimation = 2;


            float rotdir = GetRotByDir(Projectile.rotation, owner.direction);
            owner.ChangeItemRotation(rotdir, false);
            //60帧架盾，GP后切换为40帧

            float BeginP = MathHelper.Pi / 6;
            if (Phase == SwingBladePhase.Begin)
            {
                Projectile.ai[1]++;
                Projectile.rotation = MathHelper.Lerp(BeginP, BeginP - MathHelper.Pi, Projectile.ai[1] / 15f);
                StringLen = MathHelper.Lerp(0, 100, Projectile.ai[1] / 15f);
                //DrawCenter = new Vector2(owner.direction, 0) * MathHelper.Lerp(0, 10, Projectile.ai[1] / 15f);
                if (Projectile.ai[1] >= 15)
                {
                    Projectile.ai[1] = 0;
                    Phase = SwingBladePhase.Swing;
                    //HasTrail = true;

                    for (int i = 0; i < 3; i++)
                    {
                        float radian = StringLen + 10 + 20 * Main.rand.NextFloat();
                        float iniPhase = Main.rand.NextFloat() * MathHelper.TwoPi;
                        float vel = Main.rand.NextFloat() * 0.6f + 0.6f;
                        float scale = Main.rand.NextFloat() * 1.5f + 1.5f;
                        float rotateRadian = Main.rand.NextFloat() * MathHelper.TwoPi;
                        float yModifier = Main.rand.NextFloat() * 0.1f + 0.9f;
                        SwingBladeRoundingProj.SummonProj(owner, Vector2.Zero, Color.Cyan, radian, rotateRadian, iniPhase, yModifier, -vel, scale, owner.direction);
                    }
                }
            }
            else if (Phase == SwingBladePhase.Swing)
            {
                Projectile.ai[1]++;
                Projectile.rotation -= MathHelper.TwoPi / 9.67f;
                //DrawCenter = DrawCenter.RotatedBy(-MathHelper.Pi / 13f);
                if (Projectile.ai[1] >= 120)
                {
                    Projectile.ai[1] = 0;
                    Phase = SwingBladePhase.End;
                }
            }
            else if (Phase == SwingBladePhase.End)
            {
                Projectile.ai[1]++;
                if (Projectile.ai[1] < 20)
                {
                    Projectile.rotation -= MathHelper.Pi / 14f;
                    StringLen = 100;
                }
                else if (Projectile.ai[1] < 25)
                {
                    float factor = (25 - Projectile.ai[1]) / 5f + 0.5f;
                    Projectile.rotation -= MathHelper.Pi / 5f * factor;
                    StringLen = MathHelper.Lerp(100, 1, (Projectile.ai[1] - 20) / 5f);
                    //DrawCenter *= 0.9f;
                }
                else if (Projectile.ai[1] <= 50)
                {
                    StringLen = 1;
                }
                else
                {
                    Projectile.ai[1] = 0;
                    Phase = SwingBladePhase.Default;
                }
            }
            else if (Phase == SwingBladePhase.Default)
            {
                Projectile.Kill();
                return;
            }

            /*
            if (!HasTrail)
            {
                for (int i = 0; i < TrailCount; i++)
                {
                    OldRot[i] = Projectile.rotation;
                    OldRadian[i] = StringLen;
                    OldCenter[i] = DrawCenter;
                }
            }
            else
            {
                for (int i = TrailCount - 1; i > 0; i--)
                {
                    OldRot[i] = OldRot[i - 1];
                    OldRadian[i] = OldRadian[i - 1];
                    OldCenter[i] = OldCenter[i - 1];
                }
                OldRot[0] = Projectile.rotation;
                OldRadian[0] = StringLen;
                OldCenter[0] = DrawCenter;
            }
            */

        }


        public override bool PreDraw(ref Color lightColor)
        {
            Player owner = Main.player[Projectile.owner];

            Texture2D tex = DrawUtils.GetItemTexture(owner.HeldItem.type);

            Vector2 origin = new Vector2(owner.direction >= 0 ? 0 : tex.Size().X, tex.Size().Y);

            SpriteEffects spriteEffects = owner.direction >= 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            float rot = GetRotByDir(Projectile.rotation, owner.direction);
            rot += owner.direction >= 0 ? MathHelper.Pi / 4 : MathHelper.Pi / 4 * 3;

            Vector2 EndPos = Projectile.Center + GetRotByDir(Projectile.rotation, owner.direction).ToRotationVector2() * StringLen;

            Terraria.Utils.DrawLine(Main.spriteBatch, Projectile.Center, EndPos, Color.Cyan, Color.Cyan, 2);

            /*
            if (HasTrail)
            {
                Texture2D blob = ModContent.Request<Texture2D>("WireBugMod/Images/BlobGlow").Value;

                List<CustomVertexInfo> vertexInfos = new();
                for (int i = 0; i < TrailCount - 1; i++) 
                {
                    float Mix(float x1,float x2,float t)
                    {
                        return x1 * t + x2 * (1 - t);
                    }
                    Vector2 rotVec1 = OldCenter[i] + GetRotByDir(OldRot[i], owner.direction).ToRotationVector2() * (OldRadian[i] + 24);
                    Vector2 rotVec2 = OldCenter[i] + GetRotByDir(OldRot[i], owner.direction).ToRotationVector2() * (OldRadian[i] + 16);
                    Vector2 rotVec3 = OldCenter[i] + GetRotByDir(Mix(OldRot[i], OldRot[i + 1], 0.66f), owner.direction).ToRotationVector2() * (Mix(OldRadian[i], OldRadian[i + 1], 0.66f) + 24);
                    Vector2 rotVec4 = OldCenter[i] + GetRotByDir(Mix(OldRot[i], OldRot[i + 1], 0.66f), owner.direction).ToRotationVector2() * (Mix(OldRadian[i], OldRadian[i + 1], 0.66f) + 16);
                    Vector2 rotVec5 = OldCenter[i] + GetRotByDir(Mix(OldRot[i], OldRot[i + 1], 0.33f), owner.direction).ToRotationVector2() * (Mix(OldRadian[i], OldRadian[i + 1], 0.33f) + 24);
                    Vector2 rotVec6 = OldCenter[i] + GetRotByDir(Mix(OldRot[i], OldRot[i + 1], 0.33f), owner.direction).ToRotationVector2() * (Mix(OldRadian[i], OldRadian[i + 1], 0.33f) + 16);

                    vertexInfos.Add(new CustomVertexInfo(Projectile.Center + rotVec1, Color.White, new Vector3(1 - i / (float)(TrailCount - 1), 0, 1)));
                    vertexInfos.Add(new CustomVertexInfo(Projectile.Center + rotVec2, Color.White, new Vector3(1 - i / (float)(TrailCount - 1), 1, 1)));
                    vertexInfos.Add(new CustomVertexInfo(Projectile.Center + rotVec3, Color.White, new Vector3(1 - (i + 0.33f) / (TrailCount - 1), 0, 1)));
                    vertexInfos.Add(new CustomVertexInfo(Projectile.Center + rotVec4, Color.White, new Vector3(1 - (i + 0.33f) / (TrailCount - 1), 1, 1)));
                    vertexInfos.Add(new CustomVertexInfo(Projectile.Center + rotVec5, Color.White, new Vector3(1 - (i + 0.66f) / (TrailCount - 1), 0, 1)));
                    vertexInfos.Add(new CustomVertexInfo(Projectile.Center + rotVec6, Color.White, new Vector3(1 - (i + 0.66f) / (TrailCount - 1), 1, 1)));
                }
                DrawUtils.DrawTrail(blob, vertexInfos, Main.spriteBatch, Color.Cyan, BlendState.Additive);
            }
            */

            Main.spriteBatch.Draw(tex,
                EndPos - Main.screenPosition,
                null,
                lightColor * (1 - owner.immuneAlpha / 255f),
                rot,
                origin,
                Projectile.scale * owner.GetAdjustedItemScale(owner.HeldItem),
                spriteEffects,
                0);

            return false;
        }

        private void GenDust(Vector2 Pos, float Speed, float scale)
        {
            Dust dust = Dust.NewDustDirect(Pos, 1, 1, DustID.WhiteTorch);
            dust.color = Color.Cyan;
            dust.velocity = (MathHelper.TwoPi * Main.rand.NextFloat()).ToRotationVector2() * Speed;
            dust.position = Pos;
            dust.noGravity = true;
            dust.scale = scale;
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
    }
}